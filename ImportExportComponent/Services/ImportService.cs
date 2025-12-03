using ClosedXML.Excel;
using ImportExportComponent.Configuration;
using ImportExportComponent.Converters;
using ImportExportComponent.Dtos;
using ImportExportComponent.Validation.Strategies;
using ImportExportComponent.HelperServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using System.Threading;
using TableComponent.Extensions;

namespace ImportExportComponent.Services
{
    public class ImportService
    {
        private readonly EntitySetProvider _setProvider;
        private readonly RuleProvider _ruleProvider;
        private readonly DbContext _dbContext;
        private readonly ILogger<ImportService> _logger;
        private readonly ImportExportOptions _options;
        private readonly ValidationStrategyFactory _validationFactory;
        private readonly TypeConverterFactory _typeConverterFactory;
        private readonly ExcelHeaderDetector _headerDetector;

        public ImportService(
            EntitySetProvider setProvider,
            RuleProvider ruleProvider,
            DbContext dbContext,
            ILogger<ImportService> logger,
            ImportExportOptions options,
            ValidationStrategyFactory? validationFactory = null,
            TypeConverterFactory? typeConverterFactory = null,
            ExcelHeaderDetector? headerDetector = null)
        {
            _setProvider = setProvider;
            _ruleProvider = ruleProvider;
            _dbContext = dbContext;
            _logger = logger;
            _options = options;
            _validationFactory = validationFactory ?? new ValidationStrategyFactory();
            _typeConverterFactory = typeConverterFactory ?? new TypeConverterFactory();
            _headerDetector = headerDetector ?? new ExcelHeaderDetector(options);
        }

        public async Task<ExcelPreviewDto> ExtractExcelPreviewAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (file.Length == 0)
            {
                throw new ArgumentException("Fayl boş ola bilməz.", nameof(file));
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Excel faylında ən azı bir iş səhifəsi olmalıdır.");

            // Use improved header detection
            var (headers, headerRowNumber) = _headerDetector.DetectHeader(worksheet, cancellationToken);

            var records = new List<Dictionary<string, string>>(_options.MaxPreviewRecords);
            var maxPreviewRecords = _options.MaxPreviewRecords;

            foreach (var row in worksheet.RowsUsed().Where(r => r.RowNumber() > headerRowNumber))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (records.Count >= maxPreviewRecords)
                {
                    break;
                }

                var record = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);

                for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
                {
                    var header = headers[columnIndex];
                    var cell = row.Cell(columnIndex + 1);
                    var value = cell.GetValue<string>()?.Trim() ?? string.Empty;
                    record[header] = value;
                }

                // Only add non-empty records
                if (record.Values.Any(value => !string.IsNullOrWhiteSpace(value)))
                {
                    records.Add(record);
                }
            }

            return new ExcelPreviewDto
            {
                Headers = headers,
                Records = records
            };
        }

        public async Task<List<ValidatedRecordDto>> ValidateRecords(ValidateRequestDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (string.IsNullOrWhiteSpace(dto.TableName))
            {
                throw new ArgumentException("TableName dəyəri tələb olunur.", nameof(dto.TableName));
            }

            var rules = await _ruleProvider.GetRulesAsync(dto.TableName, cancellationToken);
            if (rules.Count == 0)
            {
                // No rules defined, return all records as valid
                return dto.Records.Select(record => new ValidatedRecordDto
                {
                    Records = dto.Mappings
                        .Where(mapping => record.ContainsKey(mapping.Key))
                        .ToDictionary(mapping => mapping.Value, mapping => record[mapping.Key], StringComparer.OrdinalIgnoreCase),
                    IsValid = true,
                    Errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                }).ToList();
            }

            var validations = new List<ValidatedRecordDto>(dto.Records.Count);

            foreach (var record in dto.Records)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Pre-allocate dictionary with expected capacity
                var mappedFields = new Dictionary<string, string>(dto.Mappings.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var mapping in dto.Mappings)
                {
                    if (record.TryGetValue(mapping.Key, out var value))
                    {
                        mappedFields[mapping.Value] = value;
                    }
                }

                var fieldErrors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                foreach (var rule in rules)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    mappedFields.TryGetValue(rule.Field, out var value);
                    value ??= string.Empty;

                    // Use validation strategy factory - tüm genel validasyonlar burada yapılır
                    var errors = _validationFactory.Validate(value, rule);

                    // Foreign key validation - genel yaklaşım, tüm Id alanları için
                    if (!string.IsNullOrWhiteSpace(rule.ForeignTable) && 
                        !string.IsNullOrWhiteSpace(rule.ForeignColumn) && 
                        !string.IsNullOrWhiteSpace(value))
                    {
                        try
                        {
                            var exists = await _ruleProvider.ForeignValueExistsAsync(rule.ForeignTable, rule.ForeignColumn, value, cancellationToken);
                            if (!exists)
                            {
                                errors.Add($"'{value}' dəyəri '{rule.ForeignTable}.{rule.ForeignColumn}' cədvəlində tapılmadı.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Foreign key kontrolü sırasında hata oluşursa (örn: geçersiz format), validation hatası olarak ekle
                            errors.Add($"'{value}' dəyəri '{rule.ForeignTable}.{rule.ForeignColumn}' üçün yoxlanıla bilmədi: {ex.Message}");
                        }
                    }

                    if (errors.Count > 0)
                    {
                        fieldErrors[rule.Field] = errors;
                    }
                }

                validations.Add(new ValidatedRecordDto
                {
                    Records = mappedFields,
                    IsValid = fieldErrors.Count == 0,
                    Errors = fieldErrors
                });
            }

            return validations;
        }

        /// <summary>
        /// Gets table columns metadata for a given table name
        /// </summary>
        public Task<List<TableColumnDto>> GetTableColumnsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("TableName dəyəri tələb olunur.", nameof(tableName));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var entityType = _setProvider.GetEntityType(tableName)
                ?? throw new InvalidOperationException($"Cədvəl tapılmadı: {tableName}");

            var entityMetadata = _dbContext.Model.FindEntityType(entityType);
            if (entityMetadata == null)
            {
                return Task.FromResult(new List<TableColumnDto>());
            }

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = new List<TableColumnDto>(properties.Length);
            var primaryKey = entityMetadata.FindPrimaryKey();
            var primaryKeyPropertyNames = primaryKey?.Properties.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase) 
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in properties)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip navigation properties and complex types
                if (IsNavigationOrComplexType(property))
                {
                    continue;
                }

                var propertyMetadata = entityMetadata.FindProperty(property.Name);
                if (propertyMetadata == null)
                {
                    continue;
                }

                // Skip if property has no setter (read-only)
                if (property.SetMethod == null)
                {
                    continue;
                }

                var column = new TableColumnDto
                {
                    Name = property.Name,
                    DisplayName = property.Name,
                    DataType = GetDataTypeName(property.PropertyType),
                    IsRequired = !propertyMetadata.IsNullable && !IsNullableType(property.PropertyType),
                    MaxLength = propertyMetadata.GetMaxLength(),
                    IsPrimaryKey = primaryKeyPropertyNames.Contains(property.Name),
                    IsAutoGenerated = propertyMetadata.ValueGenerated != ValueGenerated.Never
                };

                // Check for foreign key relationships
                var foreignKey = entityMetadata.GetForeignKeys()
                    .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == property.Name));

                if (foreignKey != null)
                {
                    var principalEntity = foreignKey.PrincipalEntityType;
                    var principalProperty = foreignKey.PrincipalKey.Properties.FirstOrDefault();

                    if (principalEntity != null && principalProperty != null)
                    {
                        column.ForeignTable = principalEntity.GetTableName();
                        column.ForeignColumn = principalProperty.Name;
                    }
                }

                columns.Add(column);
            }

            return Task.FromResult(columns);
        }

        private static bool IsNavigationOrComplexType(PropertyInfo property)
        {
            var type = property.PropertyType;
            
            // Fast path: check for common scalar types first
            if (type == typeof(string) || type == typeof(byte[]) || type.IsPrimitive)
            {
                return false;
            }

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Check for common value types
            if (underlyingType == typeof(Guid) || 
                underlyingType == typeof(DateTime) || 
                underlyingType == typeof(DateOnly) ||
                underlyingType == typeof(TimeOnly) ||
                underlyingType == typeof(DateTimeOffset) ||
                underlyingType == typeof(TimeSpan) ||
                underlyingType == typeof(decimal))
            {
                return false;
            }

            // Check if it's a collection (navigation property)
            if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return true;
            }

            // Check if it's a complex type (class but not string, Guid, DateTime, etc.)
            if (type.IsClass && type != typeof(string) && type != typeof(byte[]) && !type.IsPrimitive)
            {
                return true;
            }

            return false;
        }

        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static string GetDataTypeName(Type propertyType)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            return underlyingType switch
            {
                Type t when t == typeof(int) => "int",
                Type t when t == typeof(long) => "long",
                Type t when t == typeof(short) => "short",
                Type t when t == typeof(byte) => "byte",
                Type t when t == typeof(decimal) => "decimal",
                Type t when t == typeof(double) => "double",
                Type t when t == typeof(float) => "float",
                Type t when t == typeof(bool) => "bool",
                Type t when t == typeof(DateTime) => "DateTime",
                Type t when t == typeof(DateOnly) => "DateOnly",
                Type t when t == typeof(TimeOnly) => "TimeOnly",
                Type t when t == typeof(DateTimeOffset) => "DateTimeOffset",
                Type t when t == typeof(TimeSpan) => "TimeSpan",
                Type t when t == typeof(Guid) => "Guid",
                Type t when t == typeof(string) => "string",
                Type t when t == typeof(byte[]) => "byte[]",
                Type t when t.IsEnum => "enum",
                _ => underlyingType.Name
            };
        }

        public async Task<ImportResult> SaveRecords(ImportConfirmRequestDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (string.IsNullOrWhiteSpace(dto.TableName))
            {
                throw new ArgumentException("TableName dəyəri tələb olunur.", nameof(dto.TableName));
            }

            var entityType = _setProvider.GetEntityType(dto.TableName)
                ?? throw new InvalidOperationException("Cədvəl tapılmadı.");

            var totalRecords = dto.Records.Count;
            var importedRecords = 0;
            var batchSize = _options.ImportBatchSize;

            if (_options.EnableTransactionManagement)
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    importedRecords = await SaveRecordsInBatches(dto.Records, entityType, batchSize, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                importedRecords = await SaveRecordsInBatches(dto.Records, entityType, batchSize, cancellationToken);
            }

            return new ImportResult
            {
                TotalRecords = totalRecords,
                ImportedRecords = importedRecords
            };
        }

        private async Task<int> SaveRecordsInBatches(
            List<Dictionary<string, string>> records,
            Type entityType,
            int batchSize,
            CancellationToken cancellationToken)
        {
            var importedCount = 0;
            var retryAttempts = _options.MaxRetryAttempts;
            var totalRecords = records.Count;

            for (int i = 0; i < totalRecords; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batchEnd = Math.Min(i + batchSize, totalRecords);
                var entities = new List<object>(batchEnd - i);

                for (int j = i; j < batchEnd; j++)
                {
                    var record = records[j];
                    try
                    {
                        var entity = MapToEntity(record, entityType);
                        entities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Record mapping failed at index {Index}", j);
                        // Continue with other records
                    }
                }

                if (entities.Count > 0)
                {
                    var attempt = 0;
                    while (attempt < retryAttempts)
                    {
                        try
                        {
                            await _setProvider.AddRangeAsync(entities);
                            await _setProvider.SaveChangesAsync();
                            importedCount += entities.Count;
                            break;
                        }
                        catch (Exception ex) when (attempt < retryAttempts - 1)
                        {
                            attempt++;
                            _logger.LogWarning(ex, "Batch save failed, retrying (attempt {Attempt}/{MaxAttempts})", attempt, retryAttempts);
                            await Task.Delay(_options.RetryDelayMs, cancellationToken);
                        }
                    }
                }
            }

            return importedCount;
        }

        private object MapToEntity(Dictionary<string, string> mappedFields, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType)
                ?? throw new InvalidOperationException($"'{entityType.Name}' üçün nümunə yaradıla bilmədi.");

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip read-only properties
                if (property.SetMethod == null)
                {
                    continue;
                }

                // Skip Id properties that are not in mapped fields (auto-generated)
                if (property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && 
                    !mappedFields.ContainsKey(property.Name))
                {
                    continue;
                }

                // Try to get value from mapped fields
                if (!mappedFields.TryGetValue(property.Name, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                try
                {
                    var convertedValue = _typeConverterFactory.Convert(value, property.PropertyType);
                    property.SetValue(entity, convertedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Property {Property} üçün '{Value}' dəyəri konvertasiya edilə bilmədi.", property.Name, value);
                    throw;
                }
            }

            return entity;
        }
    }
}

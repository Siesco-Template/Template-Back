using ClosedXML.Excel;
using ImportExportComponent.Configuration;
using ImportExportComponent.Converters;
using ImportExportComponent.Dtos;
using ImportExportComponent.Validation.Strategies;
using ImportExportComponent.HelperServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

            var rows = worksheet.RowsUsed().ToList();
            var records = new List<Dictionary<string, string>>();
            var maxPreviewRecords = _options.MaxPreviewRecords;

            foreach (var row in rows.Where(r => r.RowNumber() > headerRowNumber))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (records.Count >= maxPreviewRecords)
                {
                    break;
                }

                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
                {
                    var header = headers[columnIndex];
                    var cell = row.Cell(columnIndex + 1);
                    var value = cell.GetValue<string>()?.Trim() ?? string.Empty;
                    record[header] = value;
                }

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
            var validations = new List<ValidatedRecordDto>(dto.Records.Count);

            foreach (var record in dto.Records)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var mappedFields = dto.Mappings
                    .Where(mapping => record.ContainsKey(mapping.Key))
                    .ToDictionary(mapping => mapping.Value, mapping => record[mapping.Key], StringComparer.OrdinalIgnoreCase);

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

            for (int i = 0; i < records.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = records.Skip(i).Take(batchSize).ToList();
                var entities = new List<object>();

                foreach (var record in batch)
                {
                    try
                    {
                        var entity = MapToEntity(record, entityType);
                        entities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Record mapping failed at index {Index}", i + batch.IndexOf(record));
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
                if (property.SetMethod is null || (property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && !mappedFields.ContainsKey(property.Name)))
                {
                    continue;
                }

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

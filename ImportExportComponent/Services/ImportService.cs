using ClosedXML.Excel;
using ImportExportComponent.Dtos;
using ImportExportComponent.Enums;
using ImportExportComponent.HelperServices;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using TableComponent.Extensions;

namespace ImportExportComponent.Services
{
    public class ImportService(EntitySetProvider setProvider, RuleProvider ruleProvider)
    {
        private readonly EntitySetProvider _setProvider = setProvider;
        private readonly RuleProvider _ruleProvider = ruleProvider;
        public async Task<ExcelPreviewDto> ExtractExcelPreviewAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = worksheet.RowsUsed().ToList();
            List<string> headers = null!;
            int headerRowIndex = -1;

            // Step 1: Başlıqların yerini avtomatik tap
            for (int i = 0; i < rows.Count; i++)
            {
                var cellValues = rows[i].CellsUsed().Select(c => c.GetValue<string>()?.Trim()).ToList();

                if (cellValues.Count >= 2 && cellValues.All(v => !string.IsNullOrWhiteSpace(v) && v.Length < 100))
                {
                    headers = cellValues;
                    headerRowIndex = i;
                    break;
                }
            }

            if (headers == null || headerRowIndex == -1)
                throw new Exception("Başlıq sətri tapılmadı.");

            // Step 2: Qalan məlumatları oxu
            var records = new List<Dictionary<string, string>>();
            for (int i = headerRowIndex + 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var cells = row.Cells(1, headers.Count).ToList();
                var record = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count; j++)
                {
                    var value = j < cells.Count ? cells[j].GetValue<string>()?.Trim() ?? "" : "";
                    record[headers[j]] = value;
                }

                if (record.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                    records.Add(record);
            }

            return new ExcelPreviewDto
            {
                Headers = headers,
                Records = records
            };
        }

        public async Task<List<ValidatedRecordDto>> ValidateRecords(ValidateRequestDto dto)
        {
            var rules = await _ruleProvider.GetRulesAsync(dto.TableName);
            var validated = new List<ValidatedRecordDto>();

            foreach (var record in dto.Records)
            {
                var mappedFields = dto.Mappings
                    .Where(m => record.ContainsKey(m.Key))
                    .ToDictionary(m => m.Value, m => record[m.Key]);

                var errors = new Dictionary<string, List<string>>();

                foreach (var rule in rules)
                {
                    if (rule.Field.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!mappedFields.TryGetValue(rule.Field, out var value))
                        value = "";

                    var fieldErrors = new List<string>();

                    if (rule.IsRequired && string.IsNullOrWhiteSpace(value))
                        fieldErrors.Add("Boş ola bilməz.");

                    if (!string.IsNullOrWhiteSpace(value) && rule.MaxLength.HasValue && value.Length > rule.MaxLength.Value)
                        fieldErrors.Add($"Maksimum uzunluq {rule.MaxLength.Value} simvoldur.");

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        switch (rule.DataType)
                        {
                            case ValidationDataType.Int:
                                if (!int.TryParse(value, out _))
                                    fieldErrors.Add("Rəqəm olmalıdır.");
                                break;

                            case ValidationDataType.Decimal:
                                if (!decimal.TryParse(value, out _))
                                    fieldErrors.Add("Onluq rəqəm olmalıdır.");
                                break;

                            case ValidationDataType.Date:
                                if (!DateTime.TryParse(value, out _))
                                    fieldErrors.Add("Tarix formatında olmalıdır.");
                                break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(rule.ForeignTable) && !string.IsNullOrWhiteSpace(rule.ForeignColumn))
                    {
                        var exists = await _ruleProvider.ForeignValueExistsAsync(rule.ForeignTable, rule.ForeignColumn, value);
                        if (!exists)
                            fieldErrors.Add($"'{value}' dəyəri '{rule.ForeignTable}.{rule.ForeignColumn}' cədvəlində tapılmadı.");
                    }

                    if (fieldErrors.Count != 0)
                        errors[rule.Field] = fieldErrors;
                }

                validated.Add(new ValidatedRecordDto
                {
                    Records = mappedFields,
                    IsValid = errors.Count == 0,
                    Errors = errors
                });
            }

            return validated;
        }

        public async Task<ImportResult> SaveRecords(ImportConfirmRequestDto dto)
        {
            var entityType = _setProvider.GetEntityType(dto.TableName) ?? throw new Exception("Cədvəl tapılmadı.");

            var entities = dto.Records.Select(record =>
                MapToEntity(record, entityType) // Artıq mapping edilmiş `record` istifadə olunur
            ).ToList();

            await _setProvider.AddRangeAsync(entities);
            await _setProvider.SaveChangesAsync();

            return new ImportResult
            {
                TotalRecords = entities.Count,
                ImportedRecords = entities.Count
            };
        }

        private object MapToEntity(Dictionary<string, string> mappedFields, Type entityType)
        {
            var entity = Activator.CreateInstance(entityType) ?? throw new Exception("Error");

            foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && !mappedFields.ContainsKey(prop.Name))
                    continue;

                if (mappedFields.TryGetValue(prop.Name, out var valueStr) && !string.IsNullOrWhiteSpace(valueStr))
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var convertedValue = Convert.ChangeType(valueStr, targetType);
                        prop.SetValue(entity, convertedValue);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return entity;
        }
    }
}
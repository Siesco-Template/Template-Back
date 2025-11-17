using ImportExportComponent.Enums;
using System.Globalization;

namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Validates data type constraints
    /// </summary>
    public class DataTypeValidationStrategy : IValidationStrategy
    {
        public List<string> Validate(string value, Dtos.FieldValidationRule rule)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value) || !rule.DataType.HasValue)
            {
                return errors;
            }

            var isValid = rule.DataType.Value switch
            {
                ValidationDataType.Int => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
                ValidationDataType.Decimal => decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
                ValidationDataType.Date => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
                                          // Excel serial date support (e.g., 44927)
                                          (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serialDate) && serialDate > 0),
                ValidationDataType.Guid => Guid.TryParse(value, out _),
                ValidationDataType.String => true,
                _ => true
            };

            if (!isValid)
            {
                errors.Add($"{rule.DataType} formatında olmalıdır.");
            }

            return errors;
        }
    }
}



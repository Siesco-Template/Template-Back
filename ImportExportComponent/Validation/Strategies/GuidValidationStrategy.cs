using ImportExportComponent.Dtos;
using ImportExportComponent.Enums;

namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Validates Guid format
    /// </summary>
    public class GuidValidationStrategy : IValidationStrategy
    {
        public List<string> Validate(string value, FieldValidationRule rule)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                return errors;
            }

            // Check if field is Guid type (Id fields ending with "Id" or property type is Guid)
            var isGuidField = rule.Field.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
                             rule.DataType == ValidationDataType.Guid;

            if (isGuidField)
            {
                if (!Guid.TryParse(value, out _))
                {
                    errors.Add($"{rule.Field} düzgün Guid formatında olmalıdır.");
                }
            }

            return errors;
        }
    }
}


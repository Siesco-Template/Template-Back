using ImportExportComponent.Dtos;
using System.Reflection;

namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Validates enum values based on entity metadata
    /// </summary>
    public class EnumValidationStrategy : IValidationStrategy
    {
        public List<string> Validate(string value, FieldValidationRule rule)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(rule.EnumType))
            {
                return errors;
            }

            try
            {
                var enumType = Type.GetType(rule.EnumType);
                if (enumType == null || !enumType.IsEnum)
                {
                    return errors;
                }

                // Try parsing as integer first (most common case)
                if (int.TryParse(value, out var intValue))
                {
                    if (!Enum.IsDefined(enumType, intValue))
                    {
                        var validValues = string.Join(", ", Enum.GetValues(enumType).Cast<object>());
                        errors.Add($"{rule.Field} dəyəri etibarlı deyil. Etibarlı dəyərlər: {validValues}");
                    }
                }
                else
                {
                    // Try parsing as enum name
                    if (!Enum.TryParse(enumType, value, ignoreCase: true, out _))
                    {
                        var validValues = string.Join(", ", Enum.GetValues(enumType).Cast<object>());
                        errors.Add($"{rule.Field} dəyəri etibarlı deyil. Etibarlı dəyərlər: {validValues}");
                    }
                }
            }
            catch
            {
                // If enum type cannot be loaded, skip validation
            }

            return errors;
        }
    }
}


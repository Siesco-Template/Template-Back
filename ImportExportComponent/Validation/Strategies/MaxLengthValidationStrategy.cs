namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Validates maximum length constraints
    /// </summary>
    public class MaxLengthValidationStrategy : IValidationStrategy
    {
        public List<string> Validate(string value, Dtos.FieldValidationRule rule)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                return errors;
            }

            if (rule.MaxLength.HasValue && value.Length > rule.MaxLength.Value)
            {
                errors.Add($"Maksimum uzunluq {rule.MaxLength.Value} simvoldur.");
            }

            return errors;
        }
    }
}



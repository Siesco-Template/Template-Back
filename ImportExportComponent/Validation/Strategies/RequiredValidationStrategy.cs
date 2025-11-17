namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Validates required fields
    /// </summary>
    public class RequiredValidationStrategy : IValidationStrategy
    {
        public List<string> Validate(string value, Dtos.FieldValidationRule rule)
        {
            var errors = new List<string>();

            if (rule.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                errors.Add("Boş ola bilməz.");
            }

            return errors;
        }
    }
}



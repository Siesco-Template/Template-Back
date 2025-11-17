namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Interface for field validation strategies
    /// </summary>
    public interface IValidationStrategy
    {
        /// <summary>
        /// Validates a field value against the rule
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="rule">The validation rule</param>
        /// <returns>List of validation error messages, empty if valid</returns>
        List<string> Validate(string value, Dtos.FieldValidationRule rule);
    }
}



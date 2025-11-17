namespace ImportExportComponent.Validation.Strategies
{
    /// <summary>
    /// Factory for creating validation strategies
    /// </summary>
    public class ValidationStrategyFactory
    {
        private readonly List<IValidationStrategy> _strategies;

        public ValidationStrategyFactory()
        {
            _strategies = new List<IValidationStrategy>
            {
                new RequiredValidationStrategy(),
                new MaxLengthValidationStrategy(),
                new DataTypeValidationStrategy(),
                new GuidValidationStrategy(),
                new EnumValidationStrategy()
            };
        }

        /// <summary>
        /// Gets all applicable validation strategies for a rule
        /// </summary>
        public IEnumerable<IValidationStrategy> GetStrategies(Dtos.FieldValidationRule rule)
        {
            return _strategies;
        }

        /// <summary>
        /// Validates a value against a rule using all applicable strategies
        /// </summary>
        public List<string> Validate(string value, Dtos.FieldValidationRule rule)
        {
            var allErrors = new List<string>();

            foreach (var strategy in GetStrategies(rule))
            {
                var errors = strategy.Validate(value, rule);
                allErrors.AddRange(errors);
            }

            return allErrors;
        }
    }
}



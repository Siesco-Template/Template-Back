namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Factory for creating and managing type converters
    /// </summary>
    public class TypeConverterFactory
    {
        private readonly List<ITypeConverter> _converters;

        public TypeConverterFactory()
        {
            _converters = new List<ITypeConverter>
            {
                new StringTypeConverter(),
                new EnumTypeConverter(),
                new GuidTypeConverter(),
                new DateTimeTypeConverter(),
                new DefaultTypeConverter() // Must be last as fallback
            };
        }

        /// <summary>
        /// Gets the appropriate converter for the target type
        /// </summary>
        public ITypeConverter GetConverter(Type targetType)
        {
            return _converters.FirstOrDefault(c => c.CanConvert(targetType))
                ?? _converters.Last(); // Fallback to DefaultTypeConverter
        }

        /// <summary>
        /// Converts a string value to the target type
        /// </summary>
        public object? Convert(string value, Type targetType)
        {
            var converter = GetConverter(targetType);
            return converter.Convert(value, targetType);
        }
    }
}



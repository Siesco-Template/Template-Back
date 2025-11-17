namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Converter for string types
    /// </summary>
    public class StringTypeConverter : ITypeConverter
    {
        public bool CanConvert(Type targetType)
        {
            return targetType == typeof(string);
        }

        public object? Convert(string value, Type targetType)
        {
            return value;
        }
    }
}



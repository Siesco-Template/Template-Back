namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Converter for enum types
    /// </summary>
    public class EnumTypeConverter : ITypeConverter
    {
        public bool CanConvert(Type targetType)
        {
            return targetType.IsEnum;
        }

        public object? Convert(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Enum.Parse(targetType, value, ignoreCase: true);
        }
    }
}



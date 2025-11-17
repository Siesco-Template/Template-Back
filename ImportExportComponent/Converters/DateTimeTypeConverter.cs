using System.Globalization;

namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Converter for DateTime, DateOnly, and TimeOnly types
    /// </summary>
    public class DateTimeTypeConverter : ITypeConverter
    {
        public bool CanConvert(Type targetType)
        {
            return targetType == typeof(DateTime) ||
                   targetType == typeof(DateOnly) ||
                   targetType == typeof(TimeOnly) ||
                   targetType == typeof(DateTime?) ||
                   targetType == typeof(DateOnly?) ||
                   targetType == typeof(TimeOnly?);
        }

        public object? Convert(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return underlyingType switch
            {
                Type t when t == typeof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.None),
                Type t when t == typeof(DateOnly) => DateOnly.Parse(value, CultureInfo.InvariantCulture),
                Type t when t == typeof(TimeOnly) => TimeOnly.Parse(value, CultureInfo.InvariantCulture),
                _ => null
            };
        }
    }
}



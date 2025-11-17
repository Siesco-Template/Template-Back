using System.ComponentModel;
using System.Globalization;

namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Default converter using TypeDescriptor and Convert.ChangeType
    /// </summary>
    public class DefaultTypeConverter : ITypeConverter
    {
        public bool CanConvert(Type targetType)
        {
            // This converter handles all types as a fallback
            return true;
        }

        public object? Convert(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var converter = TypeDescriptor.GetConverter(underlyingType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            }

            return System.Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
    }
}



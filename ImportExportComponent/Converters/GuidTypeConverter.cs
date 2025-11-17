namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Converter for Guid types
    /// </summary>
    public class GuidTypeConverter : ITypeConverter
    {
        public bool CanConvert(Type targetType)
        {
            return targetType == typeof(Guid);
        }

        public object? Convert(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Guid.Empty;
            }

            // TryParse kullanarak güvenli dönüştürme
            if (Guid.TryParse(value, out var guid))
            {
                return guid;
            }

            // Geçersiz Guid formatı için exception fırlat
            throw new FormatException($"'{value}' dəyəri düzgün Guid formatında deyil.");
        }
    }
}



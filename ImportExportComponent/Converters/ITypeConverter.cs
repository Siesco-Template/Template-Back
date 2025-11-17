namespace ImportExportComponent.Converters
{
    /// <summary>
    /// Interface for type converters
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Checks if this converter can handle the target type
        /// </summary>
        bool CanConvert(Type targetType);

        /// <summary>
        /// Converts a string value to the target type
        /// </summary>
        object? Convert(string value, Type targetType);
    }
}



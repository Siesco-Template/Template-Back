namespace Folder.HelperServices
{
    public static class CodeParsingHelper
    {
        public static (string prefix, int numberLength) ExtractPrefixAndLength(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));

            var parts = code.Split('_');
            if (parts.Length != 2)
                throw new FormatException($"Invalid code format: '{code}'.");

            string prefix = parts[0];
            string numberPart = parts[1];

            if (!int.TryParse(numberPart, out _))
                throw new FormatException($"Numeric part is not valid: '{numberPart}'");

            return (prefix, numberPart.Length);
        }
    }
}
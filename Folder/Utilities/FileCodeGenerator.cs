namespace Folder.Utilities
{
    public static class FileCodeGenerator
    {
        public static string GenerateNextCode(List<string> existingCodes, string prefix, int numberLength)
        {
            int maxNumber = 0;

            foreach (var code in existingCodes)
            {
                var parts = code.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            int nextNumber = maxNumber + 1;
            string nextNumberStr = nextNumber.ToString().PadLeft(numberLength, '0');

            return $"{prefix}_{nextNumberStr}";
        }
    }
}
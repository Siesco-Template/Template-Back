namespace ImportExportComponent.Dtos
{
    public class ValidateRequestDto
    {
        public string TableName { get; set; } = string.Empty;
        public List<Dictionary<string, string>> Records { get; set; } = [];
        public Dictionary<string, string> Mappings { get; set; } = [];
    }
}
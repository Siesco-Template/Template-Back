namespace ImportExportComponent.Dtos
{
    public class ValidateRequestDto
    {
        public string TableName { get; set; }
        public List<Dictionary<string, string>> Records { get; set; } = [];
        public Dictionary<string, string> Mappings { get; set; } = [];
    }
}
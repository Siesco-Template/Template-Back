namespace ImportExportComponent.Dtos
{
    public class ImportConfirmRequestDto
    {
        public string TableName { get; set; } = string.Empty;
        public List<Dictionary<string, string>> Records { get; set; } = [];
    }
}
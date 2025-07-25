namespace ImportExportComponent.Dtos
{
    public class ImportConfirmRequestDto
    {
        public string TableName { get; set; }
        public List<Dictionary<string, string>> Records { get; set; } = [];
    }
}
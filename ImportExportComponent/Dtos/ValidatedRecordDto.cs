namespace ImportExportComponent.Dtos
{
    public class ValidatedRecordDto
    {
        public Dictionary<string, string> Records { get; set; }
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = [];
    }
}
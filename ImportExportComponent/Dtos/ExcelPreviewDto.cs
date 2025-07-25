namespace ImportExportComponent.Dtos
{
    public class ExcelPreviewDto
    {
        public List<string> Headers { get; set; }
        public List<Dictionary<string, string>> Records { get; set; }
    }
}
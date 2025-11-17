using TableComponent.Entities;

namespace ImportExportComponent.Dtos
{
    public class ExportRequestDto
    {
        public TableQueryRequest TableRequest { get; set; } = null!;
        public List<ExportColumnDto> Columns { get; set; } = [];
    }

    public class ExportColumnDto
    {
        public string PropertyName { get; set; } = string.Empty;
        public string? Header { get; set; }
    }
}
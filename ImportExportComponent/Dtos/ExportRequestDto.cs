using TableComponent.Entities;

namespace ImportExportComponent.Dtos
{
    public class ExportRequestDto
    {
        //public string ConnectionId { get; set; }
        public TableQueryRequest TableRequest { get; set; }
        public List<ExportColumnDto> Columns { get; set; }
    }

    public class ExportColumnDto
    {
        public string PropertyName { get; set; }
        public string Header { get; set; }
    }
}
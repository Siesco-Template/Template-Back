using FilterComponent.Entities;
using QueryGenerator.Entities;

namespace TableComponent.Entities
{
    public class TableQueryRequest
    {
        public bool InitialFilter { get; set; }
        public string TableId { get; set; }
        public string Columns { get; set; }
        public List<FilterKeyValue>? Filters { get; set; }
        public PaginationRequest? Pagination { get; set; }
        public string? SortBy { get; set; }
        public bool? SortDirection { get; set; } // true → asc, false → desc
    }
}
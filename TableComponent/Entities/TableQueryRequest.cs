using FilterComponent.Dtos;
using FilterComponent.Entities;
using QueryGenerator.Entities;

namespace TableComponent.Entities
{
    public class TableQueryRequest
    {
        public string Columns { get; set; }
        public FilterDto FilterDto { get; set; }
        public PaginationRequest? Pagination { get; set; }
        public string? SortBy { get; set; }
        public bool? SortDirection { get; set; } // true → asc, false → desc
    }
}
using FilterComponent.Entities;

namespace FilterComponent.Dtos
{
    public class FilterDto
    {
        public string? TableId { get; set; }
        public List<FilterKeyValue>? Filters { get; set; }
    }
}
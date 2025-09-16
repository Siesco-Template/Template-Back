namespace TableComponent.Entities
{
    public class CatalogQueryRequest
    {
        public string TableId { get; set; }
        public string Columns { get; set; }
        public string? Filter { get; set; }
        public int Page { get; set; }
    }
}
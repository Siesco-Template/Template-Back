namespace TableComponent.Entities
{
    public class CatalogQueryRequest
    {
        public string TableId { get; set; }
        public string Columns { get; set; }
        public string? Filter { get; set; }
        public int Page { get; set; }
    }

    public class CatalogFilterRequest
    {
        public string TableId { get; set; } = string.Empty;
        public string CatalogColumn { get; set; }
        public string CatalogValue { get; set; }
        public string Columns { get; set; } = string.Empty;
    }
}
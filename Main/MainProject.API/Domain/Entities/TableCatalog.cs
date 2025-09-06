namespace MainProject.API.Domain.Entities
{
    public class TableCatalog : BaseEntity
    {
        public string TableId { get; set; }
        public string CatalogPath { get; set; }
        public string CatalogId { get; set; }
    }
}
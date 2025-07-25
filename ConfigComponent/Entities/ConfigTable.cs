namespace ConfigComponent.Entities
{
    public class ConfigTable
    {
        public string TableId { get; set; }
        public List<ColumnDefinition> Columns { get; set; } = [];
        public HeaderConfig Header { get; set; }
        public RowConfig Row { get; set; }
    }
}
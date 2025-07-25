using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConfigComponent.Entities
{
    public class Config
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRequired]
        public string UserId { get; set; }
        public List<ConfigTable> Tables { get; set; } = [];
    }

    public class ColumnDefinition
    {
        public string ColumnId { get; set; } // Məsələn: "name", "email", "price"
        public ColumnConfig Config { get; set; }
    }
}
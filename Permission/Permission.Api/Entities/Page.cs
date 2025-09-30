using MongoDB.Bson.Serialization.Attributes;
using Permission.Api.Entities.Base;

namespace Permission.Api.Entities
{
    public class Page : BaseEntity
    {
        [BsonElement("key")]
        public string Key { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("actions")]
        public List<Action> Actions { get; set; } = [];
    }
}
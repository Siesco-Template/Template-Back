using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Folder.Entities
{
    public class BaseFile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string SqlId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("createDate")]
        public DateTime CreateDate { get; set; }
    }
}
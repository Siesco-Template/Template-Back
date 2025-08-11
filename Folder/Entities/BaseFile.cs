using MongoDB.Bson.Serialization.Attributes;

namespace Folder.Entities
{
    public abstract class BaseFile
    {
        public Guid Id { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("createDate")]
        public DateTime CreateDate { get; set; }
    }
}

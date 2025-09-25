using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Folder.Entities
{
    public class FolderEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("icon")]
        public string Icon { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("path")]
        public string Path { get; set; }

        [BsonElement("createDate")]
        public DateTime CreateDate { get; set; }

        [BsonElement("updateDate")]
        public DateTime UpdateDate { get; set; }

        [BsonElement("children")]
        public List<FolderEntity> Children { get; set; } = [];

        [BsonElement("files")]
        public List<BaseFile> Files { get; set; } = [];
    }
}
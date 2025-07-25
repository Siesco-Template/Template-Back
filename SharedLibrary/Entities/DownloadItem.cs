using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SharedLibrary.Entities
{
    public class DownloadItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
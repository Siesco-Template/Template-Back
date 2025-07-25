using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Permission.Api.Entities.Base
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}

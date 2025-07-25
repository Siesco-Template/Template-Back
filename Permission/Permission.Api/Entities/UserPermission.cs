using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Permission.Api.Entities.Base;

namespace Permission.Api.Entities
{
    public class UserPermission : BaseEntity
    {
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("isBlocked")]
        public bool IsBlocked { get; set; }

        [BsonElement("permissions")]
        public List<UserPagePermission> Permissions { get; set; } = new();
    }

    public class UserPagePermission
    {
        [BsonElement("pageKey")]
        public string PageKey { get; set; }

        [BsonElement("actionKeys")]
        public List<string> ActionKeys { get; set; } = new(); 
    }
}

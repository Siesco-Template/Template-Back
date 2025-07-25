using MongoDB.Bson.Serialization.Attributes;
using Permission.Api.Entities.Base;

namespace Permission.Api.Entities
{
    public class Action : BaseEntity, IEquatable<Action>
    {
        [BsonElement("key")]
        public string Key { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        public bool Equals(Action? other)
        {
            if (other == null) return false;
            return Key == other.Key && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Action);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Name);
        }
    }
}

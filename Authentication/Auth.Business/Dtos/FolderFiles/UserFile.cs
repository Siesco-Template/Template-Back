using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Folder.Entities;

namespace Auth.Core.Dtos.FolderFiles
{
    /// <summary>
    /// Folder ile inteqrasiya modeli (folder icerisinde saxlanacaq nested user modeli)
    /// </summary>
    public class UserFile : BaseFile
    {
        //[BsonRepresentation(BsonType.String)] // Guid-i string kimi serialize/de-serialize edir
        //public Guid Id { get; set; } 
        [BsonElement("fullName")]
        public string FullName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }
    }
}
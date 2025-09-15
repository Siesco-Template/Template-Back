using Folder.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace MainProject.API.Business.Dtos.FolderFiles
{
    public class ReportFile : BaseFile
    {
        [BsonElement("number")]
        public string Number { get; set; }

        [BsonElement("organizationName")]
        public string OrganizationName { get; set; }
    }
}
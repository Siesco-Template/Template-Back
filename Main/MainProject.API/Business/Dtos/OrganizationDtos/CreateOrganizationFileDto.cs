namespace MainProject.API.Business.Dtos.OrganizationDtos
{
    public class CreateOrganizationFileDto
    {
        public string Name { get; set; }
        public string StructuralUnit { get; set; }
        public int Code { get; set; }
        public string VOEN { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FolderPath { get; set; }
    }
}
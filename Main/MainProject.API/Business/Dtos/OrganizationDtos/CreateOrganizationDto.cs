namespace MainProject.API.Business.Dtos.OrganizationDtos
{
    public class CreateOrganizationDto
    {
        public string Name { get; set; }
        public string StructuralUnit { get; set; }
        public int Code { get; set; }
        public string VOEN { get; set; }
    }
}
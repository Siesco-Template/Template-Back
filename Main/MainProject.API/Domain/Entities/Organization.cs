namespace MainProject.API.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; }
        public string StructuralUnit { get; set; }
        public int Code { get; set; }
        public string VOEN { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Report>? Reports { get; set; }
    }
}
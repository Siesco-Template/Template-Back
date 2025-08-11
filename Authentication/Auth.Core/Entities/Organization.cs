namespace Auth.Core.Entities
{
    public class Organization : BaseEntity
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string StructuralUnit { get; set; }
        public int Code { get; set; }
        public string VOEN { get; set; }
        public string TreasuryName { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Report>? Reports { get; set; }
    }
}
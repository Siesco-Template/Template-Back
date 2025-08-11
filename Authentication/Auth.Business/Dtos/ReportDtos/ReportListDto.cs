using SharedLibrary.Enums;

namespace Auth.Business.Dtos.ReportDtos
{
    public class ReportListDto
    {
        public Guid ReportId { get; set; }
        public string Number { get; set; }
        public DateTime CompileDate { get; set; }
        public Term Term { get; set; }
        public ReportStatus ReportStatus { get; set; }
        public string OrganizationName { get; set; }
        public string StructuralUnit { get; set; }
        public int OrganizationCode { get; set; }
        //public DateTime AroundDate { get; set; }
    }
}
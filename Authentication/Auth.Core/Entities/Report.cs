using AFMISMain.Core.Entities;
using SharedLibrary.Enums;

namespace Auth.Core.Entities
{
    public class Report : BaseEntity
    {
        public string Number { get; set; }
        public DateTime CompileDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Term Term { get; set; }
        public string? ClassificationCode1 { get; set; }
        public string? ClassificationCode2 { get; set; }

        public string? FuntionalClassificationCode { get; set; }
        public ReportStatus ReportStatus { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public ICollection<ReportDetail>? ReportDetails { get; set; }
    }
}
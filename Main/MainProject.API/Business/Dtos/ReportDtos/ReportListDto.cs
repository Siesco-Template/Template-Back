using SharedLibrary.Enums;

namespace MainProject.API.Business.Dtos.ReportDtos
{
    public class ReportListDto
    {
        public Guid ReportId { get; set; }
        public string Number { get; set; }
        public DateTime CompileDate { get; set; }
        public Term Term { get; set; }
        public ReportStatus ReportStatus { get; set; }
    }
}
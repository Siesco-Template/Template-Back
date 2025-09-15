using MainProject.API.Business.Dtos.ReportDtos;
using SharedLibrary.Enums;

namespace MainProject.API.Business.Dtos.ReportFileDtos
{
    public class CreateReportFileDto
    {
        public string Number { get; set; }
        public DateTime CompileDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Term Term { get; set; }

        public string? ClassificationCode1 { get; set; }
        public string? ClassificationCode2 { get; set; }
        public string? FuntionalClassificationCode { get; set; }

        public Guid OrganizationId { get; set; }

        public ICollection<CreateReportDetail> ReportDetails { get; set; }
        public string FolderPath { get; set; }
    }
}
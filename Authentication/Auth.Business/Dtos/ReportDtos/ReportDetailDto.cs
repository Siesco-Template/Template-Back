using SharedLibrary.Enums;

namespace Auth.Business.Dtos.ReportDtos
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime CompileDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Term Term { get; set; }
        public string? ClassificationCode1 { get; set; }
        public string? ClassificationCode2 { get; set; }
        public string? FuntionalClassificationCode { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public List<ReportDetailDto> ReportDetails { get; set; }

    }

    public class ReportDetailDto
    {
        public string DetailTitle { get; set; }
        public int DetailCode { get; set; }
        public decimal EstimateAmount { get; set; }

        //Maaliyələşmə məbləği
        public decimal FinancingAmount { get; set; }

        //Kassa xərc
        public decimal CheckoutAmount { get; set; }

        //Faktiki xərc
        public decimal ActualAmount { get; set; }
    }
}
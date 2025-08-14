namespace MainProject.API.Domain.Entities
{
    public class ReportDetail : BaseEntity
    {
        public Guid DetailId { get; set; }
        public Detail Detail { get; set; }

        public Guid ReportId { get; set; }
        public Report Report { get; set; }

        public decimal EstimateAmount { get; set; }
        public decimal FinancingAmount { get; set; }
        public decimal CheckoutAmount { get; set; }
        public decimal ActualAmount { get; set; }
    }
}
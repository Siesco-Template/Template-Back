namespace ImportExportComponent.Dtos
{
    public class BudgetSheetDto
    {
        public string OrganizationName { get; set; }
        public Guid OrganizationId {  get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CompileDate {  get; set; }
        public string FunctionalClassificationCode { get; set; }
        public string AdministrativeClassificationCode { get; set; }
        public List<BudgetRecord> Records { get; set; }
    }

    public class BudgetRecord
    {
        public string Indicator { get; set; }
        public string EconomicClassificationCode { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal FinancingAmount { get; set; }
        public decimal CashExecution { get; set; }
        public decimal ActualExpense { get; set; }
    }
}
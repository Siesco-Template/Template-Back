namespace MainProject.API.Business.Dtos.SaleDtos
{
    public class SaleListDto
    {
        public Guid Id { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? CargoType { get; set; }
        public string UserFullName { get; set; }
        public string DepartmentName { get; set; }
        public string PayStatus { get; set; }
        public List<SaleDetailDto> SaleDetails { get; set; }
    }

    public class SaleDetailDto
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}
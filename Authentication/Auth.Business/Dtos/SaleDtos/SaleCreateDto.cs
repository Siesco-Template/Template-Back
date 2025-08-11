using Auth.Core.Enums;

namespace Auth.Business.Dtos.SaleDtos
{
    public class SaleCreateDto
    {
        public DateTime SaleDate { get; set; }
        public DateTime? PayDate { get; set; }
        public string? CargoType { get; set; }
        public Guid UserId { get; set; }
        public Guid DepartmentId { get; set; }
        public PayStatus PayStatus { get; set; }
        public List<SaleDetailCreateDto> SaleDetails { get; set; }
    }

    public class SaleDetailCreateDto
    {
        public int Quantity { get; set; }
        public Guid ServiceId { get; set; }
    }
}
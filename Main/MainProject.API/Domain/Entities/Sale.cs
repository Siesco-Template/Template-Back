using MainProject.API.Domain.Enums;

namespace MainProject.API.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public DateTime SaleDate { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Service { get; set; }
        public string? CargoType { get; set; } // length 64

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        public PayStatus PayStatus { get; set; }
    }
}
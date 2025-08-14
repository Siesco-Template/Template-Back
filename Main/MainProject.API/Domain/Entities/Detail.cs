namespace MainProject.API.Domain.Entities
{
    public class Detail : BaseEntity
    {
        public string Title { get; set; }
        public int Code { get; set; }

        public Guid? ParentDetailId { get; set; }
        public Detail? ParentDetail { get; set; }

        public ICollection<Detail>? Childrens { get; set; }

        public ICollection<ReportDetail>? ReportDetails { get; set; }
    }
}
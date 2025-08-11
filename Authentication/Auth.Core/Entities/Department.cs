namespace Auth.Core.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<Sale> Sales { get; set; }
    }
}
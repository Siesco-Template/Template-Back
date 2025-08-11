namespace Auth.Core.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
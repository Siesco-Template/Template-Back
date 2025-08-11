using SharedLibrary.Enums;

namespace Auth.Core.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public string PhoneNumber { get; set; }
        //Asan imza nömrəsi
        public string? SignatureNumber { get; set; }
        public UserRole UserRole { get; set; }
        public bool IsBlock { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
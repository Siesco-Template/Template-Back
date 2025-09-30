using SharedLibrary.Enums;

namespace Auth.Business.Dtos
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsBlock { get; set; }
        public DateTime? LockDownDate { get; set; }
        public string? BlockInformation { get; set; }
        public UserRole UserRole { get; set; }
    }
}
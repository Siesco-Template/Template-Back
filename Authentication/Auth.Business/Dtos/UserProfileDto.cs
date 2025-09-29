using SharedLibrary.Enums;

namespace Auth.Business.Dtos
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public UserRole UserRole { get; set; }
    }
}
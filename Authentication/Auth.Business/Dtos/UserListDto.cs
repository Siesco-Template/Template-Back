using SharedLibrary.Enums;

namespace Auth.Business.Dtos
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsBlock { get; set; }
        public UserRole AppUserRole { get; set; }
    }
}
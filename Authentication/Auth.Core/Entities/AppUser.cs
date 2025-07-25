using SharedLibrary.Enums;

namespace Auth.Core.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        //Asan imza nomresi
        public string? SignatureNumber { get; set; }
        public UserRole UserRole { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? RefreshTokenExpireDate { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? LockDownDate { get; set; }
        public bool IsBlock { get; set; }
        public string? BlockInformation { get; set; }

        public PasswordToken PasswordToken { get; set; }
        public ICollection<LoginLog> LoginLogs { get; set; }
    }
}
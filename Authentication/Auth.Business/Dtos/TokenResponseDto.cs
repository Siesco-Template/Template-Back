using SharedLibrary.Enums;

namespace Auth.Business.Dtos
{
    public class TokenResponseDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public UserRole UserRole { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        public string RefreshToken { get; set; }
        //public string? UserImage { get; set; }
    }
}
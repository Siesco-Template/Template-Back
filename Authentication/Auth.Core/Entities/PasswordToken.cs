namespace Auth.Core.Entities
{
    public class PasswordToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }

        //public int? PasswordTokenRequestCount { get; set; }
        public DateTime ExpireDate { get; set; }

        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
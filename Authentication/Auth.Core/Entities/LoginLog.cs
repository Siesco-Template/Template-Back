namespace Auth.Core.Entities
{
    public class LoginLog
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsSucceed { get; set; }
        //public string? IP { get; set; }

        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
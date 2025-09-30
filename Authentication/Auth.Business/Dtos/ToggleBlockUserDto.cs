namespace Auth.Business.Dtos
{
    public class ToggleBlockUserDto
    {
        public Guid UserId { get; set; }
        public string? BlockInformation { get; set; }
        public DateTime? LockDownDate { get; set; }
    }
}
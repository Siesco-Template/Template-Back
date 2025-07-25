namespace SharedLibrary.Events
{
    public class UserChangeStatusPermissionEvent
    {
        public string UserId { get; set; }
        public bool IsActive { get; set; }
    }
}

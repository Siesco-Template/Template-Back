namespace SharedLibrary.Events
{
    public class UserUpdateFullNamePermissionEvent
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
    }
}

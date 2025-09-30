using SharedLibrary.Dtos.PermissionDtos;

namespace SharedLibrary.Events
{
    public class UserRegisteredPermissionEvent
    {
        public Guid UserId { get; set; }
        public bool IsBlocked { get; set; }
        public string FullName { get; set; }
        public List<PageDto> Pages { get; set; }
    }
}
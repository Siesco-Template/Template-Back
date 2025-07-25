using SharedLibrary.Dtos.PermissionDtos;

namespace Permission.Api.Dtos
{
    public class UserPermissionsDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public List<UserPermissionPageDto> Permissions { get; set; } = new();
    }

    public class UserPermissionPageDto
    {
        public string PageKey { get; set; }
        public List<string> ActionKeys { get; set; } = new();
    }
}

using SharedLibrary.Dtos.PermissionDtos;

namespace Permission.Api.Dtos
{
    public class UpdateUserPermissionsDto
    {
        public Guid UserId { get; set; }
        public List<UpdatePageDto> Permissions { get; set; } = new();
    }

    public class UpdatePageDto
    {
        public string Key { get; set; }
        public List<string> Actions { get; set; } = new();
    }
}

namespace Permission.Api.Dtos
{
    public class UpdateUserPermissionsDto
    {
        public Guid UserId { get; set; }
        public List<UpdatePageDto> Permissions { get; set; } = [];
    }

    public class UpdatePageDto
    {
        public string Key { get; set; }
        public List<string> Actions { get; set; } = [];
    }
}
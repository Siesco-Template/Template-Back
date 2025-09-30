namespace Permission.Api.Dtos
{
    public class FolderWithUsersAndPermissionsDto
    {
        public string FolderName { get; set; }
        public string FolderPath { get; set; }
        public List<UserPermissionsDto> Users { get; set; } = [];
    }
}
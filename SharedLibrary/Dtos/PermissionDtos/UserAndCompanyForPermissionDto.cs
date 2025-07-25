namespace SharedLibrary.Dtos.PermissionDtos
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public bool IsBlocked { get; set; }
        public int? CompanyGroupId { get; set; }
        public List<int>? AdminGroupIds { get; set; }
    }
}

namespace Auth.Business.Dtos.UserDtos
{
    public class UserListDto
    {
        public Guid Id { get; set; }    
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
    }
}
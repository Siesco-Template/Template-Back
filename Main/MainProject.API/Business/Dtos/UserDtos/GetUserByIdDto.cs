namespace MainProject.API.Business.Dtos.UserDtos
{
    public class GetUserByIdDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}

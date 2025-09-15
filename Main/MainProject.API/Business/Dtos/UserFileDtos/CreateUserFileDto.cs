namespace MainProject.API.Business.Dtos.UserFileDtos
{
    public class CreateUserFileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FolderPath { get; set; }
    }
}
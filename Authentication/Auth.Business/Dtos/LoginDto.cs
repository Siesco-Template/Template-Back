using FluentValidation;

namespace Auth.Business.Dtos
{
    public class LoginDto
    {
        public string? EmailOrUserName { get; set; }
        public string? Password { get; set; }
    }

    public class LoginDtoValidator  : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x=> x.EmailOrUserName).NotEmpty().WithMessage("Boş ola bilməz");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Şifrə boş ola bilməz");

        }
    }
}
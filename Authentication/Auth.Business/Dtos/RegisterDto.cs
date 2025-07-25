using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Dtos
{
    public class RegisterDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? SignatureNumber { get; set; }

    }
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x=> x.FirstName.Trim())
                .NotEmpty()
                    .WithName(nameof(RegisterDto.FirstName))
                    .WithMessage("Ad boş ola bilməz")
                .Length(3,32)
                    .WithName(nameof(RegisterDto.FirstName))
                    .WithMessage("Ad uzunluğu 3-32 arasında olmalıdır");

            RuleFor(x=> x.LastName.Trim())
                .NotEmpty()
                    .WithName(nameof(RegisterDto.LastName))
                    .WithMessage("Soyad boş ola bilməz")
                .Length(3,32)
                    .WithName(nameof(RegisterDto.LastName))
                    .WithMessage("Soyad uzunluğu 3-32 arasında olmalıdır");

            RuleFor(x => x.Email)
                .EmailAddress()
                    .WithMessage("Email doğru formatda deyil");

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage("Şifrə boş ola bilməz");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                    .WithMessage("Şifrə boş ola bilməz")
                .Equal(x => x.Password)
                    .WithMessage("Şifrələr uyğunlaşmır");

            RuleFor(x=> x.PhoneNumber)
                .NotEmpty()
                    .WithMessage("Telefon nömrəsi boş ola bilməz")
                .Matches(@"^(?:\+\d{1,3})?\d{1,4}\d{7,10}$")
                    .WithMessage("Telefon nömrəsi doğru formatda deyil");

        }
    }
}

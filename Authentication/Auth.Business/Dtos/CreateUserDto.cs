using FluentValidation;
using SharedLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Dtos
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? SignatureNumber { get; set; }
        public UserRole UserRole { get; set; }
    }

    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FirstName.Trim())
                .NotEmpty()
                    .WithName(nameof(RegisterDto.FirstName))
                    .WithMessage("Ad boş ola bilməz")
                .Length(3, 32)
                    .WithName(nameof(RegisterDto.FirstName))
                    .WithMessage("Ad uzunluğu 3-32 arasında olmalıdır");

            RuleFor(x => x.LastName.Trim())
                .NotEmpty()
                    .WithName(nameof(RegisterDto.LastName))
                    .WithMessage("Soyad boş ola bilməz")
                .Length(3, 32)
                    .WithName(nameof(RegisterDto.LastName))
                    .WithMessage("Soyad uzunluğu 3-32 arasında olmalıdır");

            RuleFor(x => x.Email)
                .EmailAddress()
                    .WithMessage("Email doğru formatda deyil");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                    .WithMessage("Telefon nömrəsi boş ola bilməz")
                .Matches(@"^(?:\+\d{1,3})?\d{1,4}\d{7,10}$")
                    .WithMessage("Telefon nömrəsi doğru formatda deyil");
        }
    }
}

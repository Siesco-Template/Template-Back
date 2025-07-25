using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Dtos
{
    public class SetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    public class SetPasswordValidator : AbstractValidator<SetPasswordDto>
    {
        public SetPasswordValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithMessage("Yeni şifrə boş ola bilməz");

            RuleFor(x => x.ConfirmNewPassword)
               .NotEmpty()
                   .WithMessage("Yeni şifrənin təkrarı boş ola bilməz")
               .Equal(x=> x.NewPassword)
                   .WithMessage("Şifrələr uyğunlaşmır");

            RuleFor(x => x.Token)
                .NotEmpty()
                    .WithMessage("Token boş ola bilməz");
        }
    }
}

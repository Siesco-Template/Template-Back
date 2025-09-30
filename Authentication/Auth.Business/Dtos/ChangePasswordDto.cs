using FluentValidation;

namespace Auth.Business.Dtos
{
    public class ChangePasswordDto
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? NewConfirmPassword { get; set; }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {

            RuleFor(x => x.OldPassword)
                .NotEmpty()
                    .WithMessage("Yeni şifrə boş ola bilməz");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithMessage("Yeni şifrə boş ola bilməz");

            RuleFor(x => x.NewConfirmPassword)
                .NotEmpty()
                   .WithMessage("Yeni şifrənin təkrarı boş ola bilməz")
                .Equal(x => x.NewConfirmPassword)
                    .WithMessage("Şifrələr uyğunlaşmır");
        }
    }
}
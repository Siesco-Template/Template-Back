using FluentValidation;

namespace Auth.Business.Dtos
{
    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        public string? NewPassword { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithMessage("Yeni şifrə boş ola bilməz");
        }
    }
}
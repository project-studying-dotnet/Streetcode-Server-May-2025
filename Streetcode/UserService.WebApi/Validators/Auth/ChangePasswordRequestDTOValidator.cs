using FluentValidation;
using UserService.WebApi.DTO.Auth.Requests;

namespace UserService.WebApi.Validators.Auth;

public class ChangePasswordRequestDTOValidator : AbstractValidator<ChangePasswordRequestDTO>
{
    public ChangePasswordRequestDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required.")
            .MinimumLength(6).WithMessage("Old password must be at least 6 characters.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters.");
    }
}
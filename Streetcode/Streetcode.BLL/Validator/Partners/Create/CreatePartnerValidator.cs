using FluentValidation;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.BLL.Validator.Partners.Rules;

namespace Streetcode.BLL.Validator.Partners.Create;

public sealed class CreatePartnerValidator : AbstractValidator<CreatePartnerQuery>
{
    public CreatePartnerValidator()
    {
        RuleFor(c => c.newPartner.IsKeyPartner).NotNull();
        RuleFor(c => c.newPartner.IsVisibleEverywhere).NotNull();
        RuleFor(c => c.newPartner.Title).ValidTitle();
        RuleFor(c => c.newPartner.TargetUrl).ValidUrlOptional();

        RuleFor(c => c.newPartner.Streetcodes)
            .NotEmpty()
            .WithMessage("At least one Streetcode must be assigned.");
    }
}
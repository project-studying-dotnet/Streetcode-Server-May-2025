using FluentValidation;
using Streetcode.BLL.MediatR.Partners.Update;
using Streetcode.BLL.Validator.Partners.Rules;

namespace Streetcode.BLL.Validator.Partners.Update;

public sealed class UpdatePartnerValidator : AbstractValidator<UpdatePartnerQuery>
{
    public UpdatePartnerValidator()
    {
        RuleFor(c => c.Partner.Id).GreaterThan(0);

        RuleFor(c => c.Partner.IsKeyPartner).NotNull();
        RuleFor(c => c.Partner.IsVisibleEverywhere).NotNull();
        RuleFor(c => c.Partner.Title).ValidTitle();
        RuleFor(c => c.Partner.TargetUrl).ValidUrlOptional();

        RuleFor(c => c.Partner.Streetcodes)
            .NotEmpty()
            .WithMessage("At least one Streetcode must be assigned.");
    }
}
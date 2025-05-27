using FluentValidation;
using Streetcode.BLL.MediatR.Partners.Update;
using Streetcode.BLL.Validator;

namespace Streetcode.BLL.Validator.Partners.Update;

public sealed class UpdatePartnerValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerValidator()
    {
        RuleFor(c => c.Partner.IsKeyPartner).NotNull();
        RuleFor(c => c.Partner.IsVisibleEverywhere).NotNull();
        RuleFor(c => c.Partner.Title).ValidTitle();
        RuleFor(c => c.Partner.TargetUrl).ValidUrlOptional();
        RuleFor(c => c.Partner.Streetcodes)
            .NotEmpty()
            .WithMessage("At least one Streetcode must be assigned.");
    }
}
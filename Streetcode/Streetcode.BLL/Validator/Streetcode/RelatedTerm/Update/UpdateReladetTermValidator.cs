using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.BLL.Validator.Streetcode.RelatedTerm.Rules;

namespace Streetcode.BLL.Validator.Streetcode.RelatedTerm.Update;

public sealed class UpdateRelatedTermValidator : AbstractValidator<UpdateRelatedTermCommand>
{
    public UpdateRelatedTermValidator()
    {
        RuleFor(c => c.RelatedTerm.Id).GreaterThan(0);

        RuleFor(c => c.RelatedTerm.Word).ValidWord();
        RuleFor(c => c.RelatedTerm.TermId).ValidTermId();
    }
}
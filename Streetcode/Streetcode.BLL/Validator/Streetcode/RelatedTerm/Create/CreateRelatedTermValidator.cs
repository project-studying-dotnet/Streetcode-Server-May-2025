using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.Validator.Streetcode.RelatedTerm.Rules;

namespace Streetcode.BLL.Validator.Streetcode.RelatedTerm.Create;

public sealed class CreateRelatedTermValidator : AbstractValidator<CreateRelatedTermCommand>
{
    public CreateRelatedTermValidator()
    {
        RuleFor(c => c.RelatedTerm.Word).ValidWord();
        RuleFor(c => c.RelatedTerm.TermId).ValidTermId();
    }
}
using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.Validator;

namespace Streetcode.BLL.Validator.Streetcode.RelatedTerm.Create;

public sealed class CreateRelatedTermValidator : AbstractValidator<CreateRelatedTermCommand>
{
    public CreateRelatedTermValidator()
    {
        RuleFor(c => c.RelatedTerm.Word).NotEmpty();
        RuleFor(c => c.RelatedTerm.TermId).GreaterThan(0);
    }
}
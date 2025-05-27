using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.BLL.Validator;

namespace Streetcode.BLL.Validator.Streetcode.RelatedTerm.Update;

public sealed class UpdateRelatedTermValidator : AbstractValidator<UpdateRelatedTermCommand>
{
    public UpdateRelatedTermValidator()
    {
        RuleFor(c => c.RelatedTerm.Id).ValidId();
        RuleFor(c => c.RelatedTerm.Word).NotEmpty();
        RuleFor(c => c.RelatedTerm.TermId).GreaterThan(0);
    }
}
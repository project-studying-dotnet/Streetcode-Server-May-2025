using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;

namespace Streetcode.BLL.Validator.Streetcode.Fact.Update;

public sealed class UpdateFactValidator : AbstractValidator<UpdateFactsCommand>
{
    public UpdateFactValidator()
    {
        RuleFor(c => c.FactDTO.Id).ValidId();
        RuleFor(c => c.FactDTO.Title).ValidTitle();
        RuleFor(c => c.FactDTO.FactContent).ValidText();
        RuleFor(c => c.FactDTO.StreetcodeId).ValidId();
    }
}
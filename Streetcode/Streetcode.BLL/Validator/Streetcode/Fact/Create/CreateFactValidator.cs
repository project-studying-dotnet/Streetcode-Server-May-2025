using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;

namespace Streetcode.BLL.Validator.Streetcode.Fact.Create;

public sealed class CreateFactValidator : AbstractValidator<CreateFactCommand>
{
    public CreateFactValidator()
    {
        RuleFor(c => c.NewFact.Title).ValidTitle();
        RuleFor(c => c.NewFact.FactContent).ValidText();
        RuleFor(c => c.NewFact.StreetcodeId).ValidId();
        RuleFor(c => c.NewFact.ImageDescription).ValidImageDescription();
    }
}
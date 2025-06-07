using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;

namespace Streetcode.BLL.Validator.Streetcode.Term.Create;

public sealed class CreateTermValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermValidator()
    {
        RuleFor(cmd => cmd.Term.Title).ValidTitle();
        RuleFor(cmd => cmd.Term.Description).ValidText();
        RuleFor(cmd => cmd.Term.StreetcodeId).ValidId();
    }
}

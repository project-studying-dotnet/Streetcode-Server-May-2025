using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Text.Create;

namespace Streetcode.BLL.Validator.Streetcode.Text.Create;

public sealed class CreateTextValidator : AbstractValidator<CreateTextCommand>
{
    public CreateTextValidator()
    {
        RuleFor(c => c.CreateTextRequest.Title).ValidTitle();
        RuleFor(c => c.CreateTextRequest.TextContent).ValidText();
        RuleFor(c => c.CreateTextRequest.StreetcodeId).ValidId();
    }
}
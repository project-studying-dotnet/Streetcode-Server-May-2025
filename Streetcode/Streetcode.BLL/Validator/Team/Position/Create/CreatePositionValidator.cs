using FluentValidation;
using Streetcode.BLL.MediatR.Team.Create;

namespace Streetcode.BLL.Validator.Team.Position.Create;

public sealed class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(c => c.position.Position).ValidTitle();
    }
}

using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;

namespace Streetcode.BLL.Validator.AdditionalContent.Tag.Create;

public sealed class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(c => c.tag.Title).NotEmpty();
    }
}
using FluentValidation;
using Streetcode.BLL.MediatR.Media.Image.Create;

namespace Streetcode.BLL.Validator.Media.Image.Create;

public sealed class CreateImageValidator : AbstractValidator<CreateImageCommand>
{
    public CreateImageValidator()
    {
        RuleFor(c => c.Image.Title).ValidTitle();
        RuleFor(c => c.Image.BaseFormat).NotEmpty();
        RuleFor(c => c.Image.MimeType).NotEmpty();
        RuleFor(c => c.Image.Extension).NotEmpty();
    }
}
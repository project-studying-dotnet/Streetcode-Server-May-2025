using FluentValidation;
using Streetcode.BLL.MediatR.Media.Audio.Create;

namespace Streetcode.BLL.Validator.Media.Audio.Create;

public sealed class CreateAudioValidator : AbstractValidator<CreateAudioCommand>
{
    public CreateAudioValidator()
    {
        RuleFor(c => c.Audio.Title).ValidTitle();
        RuleFor(c => c.Audio.BaseFormat).NotEmpty();
        RuleFor(c => c.Audio.MimeType).NotEmpty();
        RuleFor(c => c.Audio.Extension).NotEmpty();
    }
}
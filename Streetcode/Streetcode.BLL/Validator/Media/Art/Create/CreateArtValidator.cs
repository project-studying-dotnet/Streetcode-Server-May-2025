using FluentValidation;
using Streetcode.BLL.MediatR.Media.Art.Create;

namespace Streetcode.BLL.Validator.Media.Art.Create;

public sealed class CreateArtValidator : AbstractValidator<CreateArtCommand>
{
    public CreateArtValidator()
    {
        RuleFor(c => c.ArtCreateRequest.Title).ValidTitle();
        RuleFor(c => c.ArtCreateRequest.Description).ValidText();
        RuleFor(c => c.ArtCreateRequest.Image).NotNull();
    }
}
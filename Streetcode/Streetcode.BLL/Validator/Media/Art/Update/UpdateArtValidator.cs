using FluentValidation;
using Streetcode.BLL.MediatR.Media.Art.Update;
namespace Streetcode.BLL.Validator.Media.Art.Update;

public sealed class UpdateArtValidator : AbstractValidator<UpdateArtCommand>
{
    public UpdateArtValidator()
    {
        RuleFor(c => c.ArtUpdateRequest.Id).ValidId();
        RuleFor(c => c.ArtUpdateRequest.Title).ValidTitle();
        RuleFor(c => c.ArtUpdateRequest.Description).ValidText();
    }
}
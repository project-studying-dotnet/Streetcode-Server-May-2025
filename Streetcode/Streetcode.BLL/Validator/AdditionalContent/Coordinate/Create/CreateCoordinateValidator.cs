using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;

namespace Streetcode.BLL.Validator.AdditionalContent.Coordinate.Create;

public sealed class CreateCoordinateValidator : AbstractValidator<CreateCoordinateCommand>
{
    public CreateCoordinateValidator()
    {
        RuleFor(c => c.StreetcodeCoordinate.Latitude).NotEmpty();
        RuleFor(c => c.StreetcodeCoordinate.Longtitude).NotEmpty();
        RuleFor(c => c.StreetcodeCoordinate.StreetcodeId).ValidId();
    }
}
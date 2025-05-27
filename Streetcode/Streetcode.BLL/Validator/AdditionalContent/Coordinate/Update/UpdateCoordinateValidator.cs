using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;

namespace Streetcode.BLL.Validator.AdditionalContent.Coordinate.Update;

public sealed class UpdateCoordinateValidator : AbstractValidator<UpdateCoordinateCommand>
{
    public UpdateCoordinateValidator()
    {
        RuleFor(c => c.StreetcodeCoordinate.Id).ValidId();
        RuleFor(c => c.StreetcodeCoordinate.Latitude).NotEmpty();
        RuleFor(c => c.StreetcodeCoordinate.Longtitude).NotEmpty();
        RuleFor(c => c.StreetcodeCoordinate.StreetcodeId).ValidId();
    }
}
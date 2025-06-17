using FluentValidation;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.Validator.Streetcode;

public class StreetcodeMainPageCreateDTOValidator : AbstractValidator<StreetcodeMainPageCreateDTO>
{
    public StreetcodeMainPageCreateDTOValidator()
    {
        RuleFor(dto => dto.StreetcodeId).GreaterThan(0);
        RuleFor(dto => dto.BriefDescription).NotEmpty().MaximumLength(1000);
    }
}
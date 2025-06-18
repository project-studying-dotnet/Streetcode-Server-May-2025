using FluentValidation;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.Validator.Streetcode;

public class StreetcodeMainPageDeleteDTOValidator : AbstractValidator<StreetcodeMainPageDeleteDTO>
{
    public StreetcodeMainPageDeleteDTOValidator()
    {
        RuleFor(dto => dto.StreetcodeId).GreaterThan(0);
    }
}
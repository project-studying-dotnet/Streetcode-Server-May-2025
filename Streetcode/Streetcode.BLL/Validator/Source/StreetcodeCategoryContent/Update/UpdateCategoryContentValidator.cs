using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;

namespace Streetcode.BLL.Validator.Source;
public class UpdateCategoryContentValidator : AbstractValidator<UpdateStreetcodeCategoryContentCommand>
{
    public UpdateCategoryContentValidator()
    {
        RuleFor(c => c.Dto.Text)
            .NotEmpty().WithMessage("Text cannot be blank")
            .MaximumLength(4000).WithMessage("Text cannot be more than 4000 characters");

        RuleFor(c => c.Dto.SourceLinkCategoryId)
            .ValidId().WithMessage("SourceLinkCategoryId cannot be blank");

        RuleFor(c => c.Dto.StreetcodeId)
            .ValidId().WithMessage("StreetcodeId cannot be blank");

        RuleFor(c => c.Dto.Id)
            .GreaterThan(0).WithMessage("Id is required for updating.");
    }
}
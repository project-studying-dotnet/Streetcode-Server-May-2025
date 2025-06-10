using FluentValidation;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

namespace Streetcode.BLL.Validator.Source;

public class CreateCategoryContentValidator : AbstractValidator<CreateStreetcodeCategoryContentCommand>
{
    public CreateCategoryContentValidator()
    {
        RuleFor(c => c.CategoryContentDto.Text)
            .NotEmpty().WithMessage("Text cannot be blank")
            .MaximumLength(4000).WithMessage("Text cannot be more than 4000 characters");

        RuleFor(c => c.CategoryContentDto.SourceLinkCategoryId)
            .ValidId().WithMessage("SourceLinkCategoryId cannot be blank");
        
        RuleFor(c => c.CategoryContentDto.StreetcodeId)
            .ValidId().WithMessage("SourceLinkCategoryId cannot be blank");
    }
}
using FluentValidation;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;

namespace Streetcode.BLL.Validator.News.Create;

public sealed class CreateTagValidator : AbstractValidator<CreateTagQuery>
{
    public CreateTagValidator()
    {
        RuleFor(q => q.tag.Title).NotEmpty();
    }
}
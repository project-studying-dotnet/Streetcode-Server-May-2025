using FluentValidation;
using Streetcode.BLL.MediatR.News.GetById;

namespace Streetcode.BLL.Validator.News.GetById;

public sealed class GetNewsByIdValidator : AbstractValidator<GetNewsByIdQuery>
{
    public GetNewsByIdValidator()
    {
        RuleFor(q => q.id).GreaterThan(0);
    }
}
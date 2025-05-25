using FluentValidation;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.BLL.Validator.News.Rules;

namespace Streetcode.BLL.Validator.News.GetByUrl;

public sealed class GetNewsByUrlValidator : AbstractValidator<GetNewsByUrlQuery>
{
    public GetNewsByUrlValidator()
    {
        RuleFor(q => q.url).ValidUrl();
    }
}
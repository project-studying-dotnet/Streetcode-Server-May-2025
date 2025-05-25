using FluentValidation;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Streetcode.BLL.Validator.News.Rules;

namespace Streetcode.BLL.Validator.News.GetNewsAndLinksByUrl;

public sealed class GetNewsAndLinksByUrlValidator : AbstractValidator<GetNewsAndLinksByUrlQuery>
{
    public GetNewsAndLinksByUrlValidator()
    {
        RuleFor(q => q.url).ValidUrl();
    }
}
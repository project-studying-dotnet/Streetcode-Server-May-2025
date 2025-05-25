using FluentValidation;
using Streetcode.BLL.MediatR.News.Update;
using Streetcode.BLL.Validator.News.Rules;

namespace Streetcode.BLL.Validator.News.Update;

public sealed class UpdateNewsValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsValidator()
    {
        RuleFor(c => c.news.Id).GreaterThan(0);

        RuleFor(c => c.news.Title).ValidTitle();
        RuleFor(c => c.news.Text).ValidText();
        RuleFor(c => c.news.URL).ValidUrl();
        RuleFor(c => c.news.CreationDate).NotInFuture();
    }
}
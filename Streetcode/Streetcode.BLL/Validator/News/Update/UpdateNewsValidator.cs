using FluentValidation;
using Streetcode.BLL.MediatR.News.Update;

namespace Streetcode.BLL.Validator.News.Update;

public sealed class UpdateNewsValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsValidator()
    {
        RuleFor(c => c.news.Id).ValidId();
        RuleFor(c => c.news.Title).ValidTitle();
        RuleFor(c => c.news.Text).ValidText();
        RuleFor(c => c.news.URL).ValidUrl();
        RuleFor(c => c.news.CreationDate).NotInFuture();
    }
}
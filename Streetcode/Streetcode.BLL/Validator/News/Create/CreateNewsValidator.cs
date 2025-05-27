using FluentValidation;
using Streetcode.BLL.MediatR.News.Create;

namespace Streetcode.BLL.Validator.News.Create;

public sealed class CreateNewsValidator : AbstractValidator<CreateNewsCommand>
{
    public CreateNewsValidator()
    {
        RuleFor(c => c.newNews.Title).ValidTitle();
        RuleFor(c => c.newNews.Text).ValidText();
        RuleFor(c => c.newNews.URL).ValidUrl();
        RuleFor(c => c.newNews.CreationDate).NotInFuture();
    }
}
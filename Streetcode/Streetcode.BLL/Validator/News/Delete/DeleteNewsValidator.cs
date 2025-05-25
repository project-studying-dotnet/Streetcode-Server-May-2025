using FluentValidation;
using Streetcode.BLL.MediatR.News.Delete;

namespace Streetcode.BLL.Validator.News.Delete;

public sealed class DeleteNewsValidator : AbstractValidator<DeleteNewsCommand>
{
    public DeleteNewsValidator()
    {
        RuleFor(c => c.id).GreaterThan(0);
    }
}
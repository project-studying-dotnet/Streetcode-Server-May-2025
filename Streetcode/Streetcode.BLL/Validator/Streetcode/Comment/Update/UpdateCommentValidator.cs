using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;

namespace Streetcode.BLL.Validator.Streetcode.Comment.Update;

public class UpdateCommentValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentValidator()
    {
        RuleFor(c => c.UpdatedComment.Id).ValidId();
        RuleFor(c => c.UpdatedComment.Text)
            .ValidText()
            .MaximumLength(1000);
    }
}
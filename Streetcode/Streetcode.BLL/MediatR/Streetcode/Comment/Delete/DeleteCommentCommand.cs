using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete;

public record DeleteCommentCommand(int CommentId) : IRequest<Result<Unit>>;
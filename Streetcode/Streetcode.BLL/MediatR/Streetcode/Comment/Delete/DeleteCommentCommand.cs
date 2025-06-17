using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete;

public record DeleteCommentCommand(int Id) 
    : IRequest<Result<Unit>>;
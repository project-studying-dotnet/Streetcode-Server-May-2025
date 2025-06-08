using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Approve;

public record ApproveCommentCommand(int CommentId) : IRequest<Result>; 
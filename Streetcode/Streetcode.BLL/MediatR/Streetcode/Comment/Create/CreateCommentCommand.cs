using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Create;

public record CreateCommentCommand(CreateCommentDTO newComment) : IRequest<Result<CommentDTO>>;

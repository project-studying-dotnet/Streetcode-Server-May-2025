using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update;

public record UpdateCommentCommand(UpdateCommentDTO UpdatedComment)
    : IRequest<Result<Unit>>;
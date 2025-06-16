using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetPending;

public record GetPendingCommentsQuery() : IRequest<Result<List<AdminCommentDTO>>>;
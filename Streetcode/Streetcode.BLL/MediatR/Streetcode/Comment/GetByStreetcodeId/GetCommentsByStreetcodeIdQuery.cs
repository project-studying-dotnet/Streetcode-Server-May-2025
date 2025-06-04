using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

public record GetCommentsByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<CommentDto>>>; 
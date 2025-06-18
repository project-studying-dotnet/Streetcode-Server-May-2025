using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetById;

public record GetCommentByIdQuery(int Id) : IRequest<Result<AdminCommentDTO>>; 
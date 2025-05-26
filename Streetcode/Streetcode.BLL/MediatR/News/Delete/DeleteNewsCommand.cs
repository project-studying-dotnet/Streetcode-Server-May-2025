using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.News.Delete;

public record DeleteNewsCommand(int id) : IRequest<Result<Unit>>;
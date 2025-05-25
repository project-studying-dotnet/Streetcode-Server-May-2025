using MediatR;
using FluentResults;

namespace Streetcode.BLL.MediatR.Media.Art.Delete;

public record DeleteArtCommand(int Id) : IRequest<Result<Unit>>;
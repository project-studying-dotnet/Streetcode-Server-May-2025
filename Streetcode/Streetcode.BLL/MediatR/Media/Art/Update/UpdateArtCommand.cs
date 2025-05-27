using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Art;

namespace Streetcode.BLL.MediatR.Media.Art.Update;

public record UpdateArtCommand(UpdateArtRequestDTO ArtUpdateRequest) : IRequest<Result<ArtDTO>>;

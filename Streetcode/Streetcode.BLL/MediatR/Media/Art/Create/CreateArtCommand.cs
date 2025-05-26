using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Art;

namespace Streetcode.BLL.MediatR.Media.Art.Create;

public record CreateArtCommand(ArtDTO newArt) : IRequest<Result<ArtDTO>>;
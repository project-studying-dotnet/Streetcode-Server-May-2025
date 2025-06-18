using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.UpdateMainPage;

public record UpdateMainCommand(StreetcodeMainPageDTO StreetcodeMainPageDTO) : IRequest<Result<Unit>>;
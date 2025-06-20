using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.MainPage.Create;

public record CreateMainStreetcodeCommand(StreetcodeMainPageCreateDTO Dto)
    : IRequest<Result<StreetcodeMainPageCreateDTO>>;
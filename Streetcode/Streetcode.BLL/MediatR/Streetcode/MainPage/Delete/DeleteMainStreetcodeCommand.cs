using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.MainPage.Delete;

public record DeleteMainStreetcodeCommand(StreetcodeMainPageDeleteDTO Dto)
    : IRequest<Result<StreetcodeMainPageDeleteDTO>>;
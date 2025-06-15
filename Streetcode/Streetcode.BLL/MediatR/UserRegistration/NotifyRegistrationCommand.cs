using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Messaging;

namespace Streetcode.BLL.MediatR.UserRegistration;

public record NotifyRegistrationCommand(UserRegisteredEventDTO Event)
    : IRequest<Result<Unit>>;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;

namespace Streetcode.BLL.MediatR.UserRegistration;

public class NotifyRegistrationHandler
    : IRequestHandler<NotifyRegistrationCommand, Result<Unit>>
{
    private readonly ILoggerService _logger;

    public NotifyRegistrationHandler(ILoggerService logger)
    {
        _logger = logger;
    }

    public Task<Result<Unit>> Handle(
        NotifyRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var data = request.Event;

        _logger.LogInformation(
            $"User registration received: UserId={data.UserId}, Email={data.Email}, RegisteredAt={data.RegisteredAt}");

        // TODO: Then use the email service here and send an email as a notification.

        return Task.FromResult(Result.Ok(Unit.Value));
    }
}
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using UserService.WebApi.Configurations;
using UserService.WebApi.DTO.Messaging;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Services.Realisations;

public class UserRegistrationPublisher : IUserRegistrationPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSettings _settings;
    private readonly ILogger<UserRegistrationPublisher> _logger;

    public UserRegistrationPublisher(
        ServiceBusClient client,
        IOptions<ServiceBusSettings> options,
        ILogger<UserRegistrationPublisher> logger)
    {
        _client = client;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task PublishUserRegisteredAsync(UserRegisteredEventDTO eventDto, CancellationToken cancellationToken)
    {
        var sender = _client.CreateSender(_settings.UserRegistrationQueueName);

        var body = JsonSerializer.Serialize(eventDto);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(body))
        {
            MessageId = Guid.NewGuid().ToString(),
            ContentType = "application/json"
        };

        _logger.LogInformation("Publishing UserRegisteredEvent: {UserId}", eventDto.UserId);

        await sender.SendMessageAsync(message, cancellationToken);
    }
}
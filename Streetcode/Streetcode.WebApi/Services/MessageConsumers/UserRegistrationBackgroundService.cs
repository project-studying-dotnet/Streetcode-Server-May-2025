using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Options;
using Streetcode.BLL.DTO.Messaging;
using Streetcode.BLL.MediatR.UserRegistration;
using Streetcode.WebApi.Configurations;
using System.Text;
using System.Text.Json;

namespace Streetcode.WebApi.Services.MessageConsumers;

public class UserRegistrationBackgroundService : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSettings _settings;
    private readonly ILogger<UserRegistrationBackgroundService> _logger;
    private readonly IMediator _mediator;

    public UserRegistrationBackgroundService(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusSettings> serviceBusOptions,
        ILogger<UserRegistrationBackgroundService> logger,
        IMediator mediator)
    {
        _serviceBusClient = serviceBusClient;
        _settings = serviceBusOptions.Value;
        _logger = logger;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _settings.UserRegistrationQueueName;
        await using var processor = _serviceBusClient.CreateProcessor(
            queueName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });

        processor.ProcessMessageAsync += ProcessMessageHandler;
        processor.ProcessErrorAsync += ProcessErrorHandler;

        _logger.LogInformation($"Starting Service Bus processor for queue '{queueName}'");
        await processor.StartProcessingAsync(stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);

        _logger.LogInformation($"Stopping Service Bus processor for queue '{queueName}'");
        await processor.StopProcessingAsync();
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Message.Body);
        var dto = JsonSerializer.Deserialize<UserRegisteredEventDTO>(json);

        if (dto == null)
        {
            _logger.LogWarning($"Received invalid message with ID: {args.Message.MessageId}");
        }
        else
        {
            // Delegate business processing to the business layer
            await _mediator.Send(
                new NotifyRegistrationCommand(
                    new Streetcode.BLL.DTO.Messaging.UserRegisteredEventDTO
                    {
                        UserId = dto.UserId,
                        Email = dto.Email,
                        Name = dto.Name,
                        Surname = dto.Surname,
                        RegisteredAt = dto.RegisteredAt
                    }
                ),
                args.CancellationToken);
        }

        // Complete the message so it is not received again
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Error in Service Bus (EntityPath: {EntityPath}, ErrorSource: {ErrorSource})",
            args.EntityPath,
            args.ErrorSource);
        return Task.CompletedTask;
    }
}
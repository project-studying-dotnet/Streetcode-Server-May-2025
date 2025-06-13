using Contracts;
using MassTransit;

namespace UserService.WebApi.Consumers;
public class TestConsumer : IConsumer<TestConnectionEvent>
{
    public Task Consume(ConsumeContext<TestConnectionEvent> context)
    {
        Console.WriteLine($"Received event: {context.Message.Message}");
        return Task.CompletedTask;
    }
}


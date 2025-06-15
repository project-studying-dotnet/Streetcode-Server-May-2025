using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using UserService.WebApi.Configurations;

namespace UserService.WebApi.Extensions;

public static class ServiceBusExtensions
{
    public static IServiceCollection AddAzureServiceBusIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ServiceBusSettings>(
            configuration.GetSection("AzureServiceBus")
        );

        services.AddSingleton(sp =>
        {
            var settings = sp
                .GetRequiredService<IOptions<ServiceBusSettings>>()
                .Value;

            return new ServiceBusClient(settings.ConnectionString);
        });

        return services;
    }
}
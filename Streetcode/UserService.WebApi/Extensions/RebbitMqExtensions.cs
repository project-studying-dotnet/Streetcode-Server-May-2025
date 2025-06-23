using MassTransit;
using UserService.WebApi.Consumers;

namespace UserService.WebApi.Extensions;
public static class RebbitMqExtensions
{
    public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.EnvironmentName == "Experimental")
        {
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.AddConsumer<TestConsumer>();
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(configuration["MessageBroker:Host"]!), h =>
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Password(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);
                });
            });
        }
        
        return services;
    }
}
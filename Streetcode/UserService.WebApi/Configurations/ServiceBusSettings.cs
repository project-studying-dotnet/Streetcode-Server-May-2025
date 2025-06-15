namespace UserService.WebApi.Configurations;

public class ServiceBusSettings
{
    public string ConnectionString { get; set; } = null!;
    public string UserRegistrationQueueName { get; set; } = null!;
}
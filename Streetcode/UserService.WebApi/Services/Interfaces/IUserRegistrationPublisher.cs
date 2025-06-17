using UserService.WebApi.DTO.Messaging;

namespace UserService.WebApi.Services.Interfaces;

public interface IUserRegistrationPublisher
{
    Task PublishUserRegisteredAsync(UserRegisteredEventDTO eventDTO, CancellationToken cancellationToken);
}
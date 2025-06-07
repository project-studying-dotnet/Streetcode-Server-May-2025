using FluentResults;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Services.Interfaces;
public interface IAuthService
{
    Task<Result<User>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken);
}


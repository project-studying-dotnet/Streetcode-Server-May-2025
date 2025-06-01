using FluentResults;
using UserService.WebApi.DTO.Users;

namespace UserService.WebApi.Services.Interfaces;
public interface IAuthService
{
    Task<Result<RegisterUserDTO>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken);
}


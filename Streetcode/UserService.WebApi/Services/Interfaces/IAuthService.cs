using FluentResults;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.DTO.Auth.Responses;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Services.Interfaces;
public interface IAuthService
{
    Task<Result<User>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken);

    Task<Result<TokenResponseDTO>> LoginAsync(LoginRequestDTO loginDTO, CancellationToken cancellationToken);

    Task<Result<TokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request, CancellationToken cancellationToken);

    Task<Result> LogoutAsync(LogoutRequestDTO request, CancellationToken cancellationToken);
}


using FluentResults;
using UserService.WebApi.DTO.Auth;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Services.Interfaces;

public interface ITokenService
{
    Task<Result<TokenResultDTO>> GenerateTokensAsync(User user, CancellationToken cancellationToken);

    Task<Result<TokenResultDTO>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
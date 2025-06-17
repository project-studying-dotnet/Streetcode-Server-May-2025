using FluentResults;
using UserService.WebApi.DTO.Auth.Responses;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Services.Interfaces;

public interface ITokenService
{
    Task<Result<TokenResponseDTO>> GenerateTokensAsync(User user, CancellationToken cancellationToken);

    Task<Result<TokenResponseDTO>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<Result<int>> DeleteRevokedRefreshTokensAsync(CancellationToken cancellationToken);
}
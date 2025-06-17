using UserService.WebApi.Entities.Auth;

namespace UserService.WebApi.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task<int> BulkRevokeExpiredTokensAsync(DateTime now, CancellationToken cancellationToken);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
﻿using Microsoft.EntityFrameworkCore;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.Entities.Auth;

namespace UserService.WebApi.Data.Repositories.Realisations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UserServiceDbContext _dbContext;

    public RefreshTokenRepository(UserServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<int> BulkRevokeExpiredTokensAsync(
        DateTime now,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => !rt.IsRevoked && rt.ExpiresAt < now)
            .ExecuteUpdateAsync(updates => updates
                    .SetProperty(rt => rt.IsRevoked, rt => true),
                cancellationToken);
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);

        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkDeleteRevokedTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
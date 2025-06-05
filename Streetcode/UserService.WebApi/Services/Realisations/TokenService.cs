using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.WebApi.Configurations;
using UserService.WebApi.Data;
using UserService.WebApi.DTO.Auth;
using UserService.WebApi.Entities.Auth;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Services.Realisations;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserServiceDbContext _dbContext;

    public TokenService(IOptions<JwtSettings> jwtOptions, UserServiceDbContext dbContext)
    {
        _jwtSettings = jwtOptions.Value;
        _dbContext = dbContext;
    }

    public async Task<Result<TokenResultDTO>> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiry = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiry,
            signingCredentials: creds
        );

        var accessTokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var refreshTokenString = GenerateRefreshTokenString();
        var refreshTokenExpiry = now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenString,
            ExpiresAt = refreshTokenExpiry,
            UserId = user.Id,
            IsRevoked = false,
            CreatedAt = now
        };

        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var resultDto = new TokenResultDTO
        {
            AccessToken = accessTokenString,
            AccessTokenExpiresAt = accessTokenExpiry,
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };

        return Result.Ok(resultDto);
    }
       
    public async Task<Result<TokenResultDTO>> RefreshTokensAsync(
        string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Fail<TokenResultDTO>("Refresh token is null or empty.");

        var existingToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (existingToken == null)
            return Result.Fail<TokenResultDTO>("Invalid refresh token.");

        if (existingToken.IsRevoked)
            return Result.Fail<TokenResultDTO>("Refresh token has been revoked.");

        if (existingToken.ExpiresAt < DateTime.UtcNow)
            return Result.Fail<TokenResultDTO>("Refresh token has expired.");

        existingToken.IsRevoked = true;
        _dbContext.RefreshTokens.Update(existingToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var user = existingToken.User;
        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Fail<bool>("Refresh token is null or empty.");

        var tokenEntity = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity == null)
            return Result.Fail<bool>("Refresh token not found.");

        if (tokenEntity.IsRevoked)
            return Result.Fail<bool>("Refresh token is already revoked.");

        tokenEntity.IsRevoked = true;
        _dbContext.RefreshTokens.Update(tokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(true);
    }

    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
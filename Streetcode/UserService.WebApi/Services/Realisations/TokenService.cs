using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.WebApi.Configurations;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.DTO.Auth.Responses;
using UserService.WebApi.Entities.Auth;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Services.Realisations;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> jwtOptions,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager,
        ILogger<TokenService> logger)
    {
        _jwtSettings = jwtOptions.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<TokenResponseDTO>> GenerateTokensAsync(User user, CancellationToken cancellationToken)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var utcNow = DateTime.UtcNow;
        var accessTokenExpiry = utcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var accessToken = CreateJwtAccessToken(user, userRoles, accessTokenExpiry);

        var refreshTokenValue = GenerateRefreshTokenString();
        var refreshTokenExpiry = utcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiry,
            UserId = user.Id,
            IsRevoked = false,
            CreatedAt = utcNow
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        var changesSaved = await _refreshTokenRepository.SaveChangesAsync(cancellationToken) > 0;
        if (changesSaved)
        {
            var tokenResponseDto = new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiry,

                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiry
            };

            return Result.Ok(tokenResponseDto);
        }
        else
        {
            return Result.Fail<TokenResponseDTO>("Failed to save RefreshToken.");
        }
    }

    public async Task<Result<TokenResponseDTO>> RefreshAccessTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Fail<TokenResponseDTO>("Refresh token is null or empty.");
        }

        var existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (existingRefreshToken == null)
        {
            return Result.Fail<TokenResponseDTO>("Invalid refresh token.");
        }

        if (existingRefreshToken.IsRevoked)
        {
            return Result.Fail<TokenResponseDTO>("Refresh token has been revoked.");
        }

        if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            await RevokeRefreshTokenAsync(existingRefreshToken.Token, cancellationToken);

            return Result.Fail<TokenResponseDTO>("Refresh token has expired.");
        }

        var user = existingRefreshToken.User!;
        var utcNow = DateTime.UtcNow;
        var accessTokenExpiry = utcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var userRoles = await _userManager.GetRolesAsync(user);

        var accessToken = CreateJwtAccessToken(user, userRoles, accessTokenExpiry);

        var tokenResponseDto = new TokenResponseDTO
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiry,

            RefreshToken = existingRefreshToken.Token,
            RefreshTokenExpiresAt = existingRefreshToken.ExpiresAt
        };

        return Result.Ok(tokenResponseDto);
    }

    public async Task<Result<bool>> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Fail<bool>("Refresh token is null or empty.");
        }

        var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (refreshTokenEntity == null)
        {
            return Result.Fail<bool>("Refresh token not found.");
        }

        if (refreshTokenEntity.IsRevoked)
        {
            return Result.Fail<bool>("Refresh token is already revoked.");
        }

        refreshTokenEntity.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(refreshTokenEntity, cancellationToken);

        var changesSaved = await _refreshTokenRepository.SaveChangesAsync(cancellationToken) > 0;
        if (changesSaved)
        {
            return Result.Ok(true);
        }
        else
        {
            return Result.Fail<bool>("Failed to save revoke RefreshToken.");
        }
    }

    public async Task<Result<int>> DeleteRevokedRefreshTokensAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Starting bulk delete of revoked refresh tokens at {DateTime.UtcNow}");

        var deletedCount = await _refreshTokenRepository
            .BulkDeleteRevokedTokensAsync(cancellationToken);

        if (deletedCount == 0)
        {
            _logger.LogInformation("No revoked refresh tokens found to delete.");
        }
        else
        {
            _logger.LogInformation($"Completed bulk delete of revoked refresh tokens. Total deleted: {deletedCount}");
        }

        return Result.Ok(deletedCount);
    }

    private string CreateJwtAccessToken(User user, IEnumerable<string> userRoles, DateTime expiresAt)
    {
        var utcNow = DateTime.UtcNow;
        var tokenClaims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        foreach (var userRole in userRoles)
        {
            tokenClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: tokenClaims,
            notBefore: utcNow,
            expires: expiresAt,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
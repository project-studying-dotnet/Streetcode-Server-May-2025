using AutoMapper;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.DTO.Auth.Responses;
using UserService.WebApi.DTO.Messaging;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Services.Realisations;

public class AuthService : IAuthService
{
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IValidator<LoginRequestDTO> _loginValidator;

    private readonly IUserRegistrationPublisher _registrationPublisher;

    public AuthService(
        IMapper mapper,
        ILogger<AuthService> logger,
        UserManager<User> userManager,
        ITokenService tokenService,
        IValidator<LoginRequestDTO> loginValidator,
        IUserRegistrationPublisher registrationPublisher)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _tokenService = tokenService;
        _loginValidator = loginValidator;
        _registrationPublisher = registrationPublisher;
    }

    public async Task<Result<User>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerUserDTO.Email);

        if (existingUser != null)
        {
            return Result.Fail<User>("User with this email already exists");
        }

        var newUser = _mapper.Map<User>(registerUserDTO);

        var result = await _userManager.CreateAsync(newUser, registerUserDTO.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to create user: {errors}");
            return Result.Fail<User>($"Failed to create user: {errors}");
        }

        var eventDto = _mapper.Map<UserRegisteredEventDTO>(newUser);

        await _registrationPublisher
            .PublishUserRegisteredAsync(eventDto, cancellationToken);

        _logger.LogInformation("Created user with id {UserId}", newUser.Id);

        return Result.Ok(newUser);
    }

    public async Task<Result<TokenResponseDTO>> LoginAsync(LoginRequestDTO loginDTO, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginDTO, cancellationToken);
        if (!validationResult.IsValid)
        {
            const string errorMsg = "Login validation failed";
            var errorDetails = string.Join("; ",
                validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

            _logger.LogError(
                "{ErrorMsg}. Details: {ErrorDetails}",
                errorMsg, errorDetails);

            return Result.Fail<TokenResponseDTO>(errorMsg)
                .WithErrors(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        _logger.LogInformation("Login attempt for email: {Email}", MaskEmail(loginDTO.Email));

        var user = await _userManager.FindByEmailAsync(loginDTO.Email);
        if (user is null)
        {
            const string errorMsg = "Invalid credentials";
            _logger.LogError("Login failed: {Error}. Email: {Email}", errorMsg, MaskEmail(loginDTO.Email));

            return Result.Fail<TokenResponseDTO>(errorMsg);
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
        if (!isValidPassword)
        {
            const string errorMsg = "Invalid credentials";
            _logger.LogError("Login failed: {Error} for user {UserId}", errorMsg, user.Id);

            return Result.Fail<TokenResponseDTO>(errorMsg);
        }

        var tokenResult = await _tokenService.GenerateTokensAsync(user, cancellationToken);
        if (tokenResult.IsFailed)
        {
            _logger.LogError(
                "Token generation failed for user {UserId}: {Errors}",
                user.Id, string.Join("; ", tokenResult.Errors.Select(e => e.Message)));

            return Result.Fail<TokenResponseDTO>("Token generation failed")
                .WithErrors(tokenResult.Errors);
        }

        _logger.LogInformation(
            "User {UserId} logged in successfully. Access token expires at: {Expiry}",
            user.Id, tokenResult.Value.AccessTokenExpiresAt);

        return tokenResult;
    }

    public async Task<Result<TokenResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO request,
        CancellationToken cancellationToken)
    {
        var refreshToken = request.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            const string errorMsg = "Refresh token is null or empty";
            _logger.LogError("Refresh token failed: {Error}", errorMsg);

            return Result.Fail<TokenResponseDTO>(errorMsg);
        }

        var result = await _tokenService.RefreshAccessTokenAsync(refreshToken, cancellationToken);
        if (result.IsFailed)
        {
            _logger.LogError(
                "Refresh token failed: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Message)));

            return Result.Fail<TokenResponseDTO>("Token refresh failed")
                .WithErrors(result.Errors);
        }

        _logger.LogInformation(
            "Token refreshed successfully for user {UserId}. New token expires at: {Expiry}",
            GetUserIdFromToken(result.Value.AccessToken),
            result.Value.AccessTokenExpiresAt);

        return result;
    }

    public async Task<Result> LogoutAsync(LogoutRequestDTO request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            const string errorMsg = "Refresh token is null or empty";
            _logger.LogError("Logout failed: {Error}", errorMsg);

            return Result.Fail(errorMsg);
        }

        var revokeResult = await _tokenService
            .RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (revokeResult.IsFailed)
        {
            _logger.LogError(
                "Logout failed when revoking refresh token: {Errors}",
                string.Join("; ", revokeResult.Errors.Select(e => e.Message))
            );

            return Result.Fail("Logout failed")
                .WithErrors(revokeResult.Errors);
        }

        _logger.LogInformation("User logged out successfully.");

        return Result.Ok();
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            return "invalid-email";
        }

        var parts = email.Split('@');
        var username = parts[0];
        var domain = parts[1];

        if (username.Length <= 2)
        {
            return $"{username[0]}...@{domain}";
        }

        return $"{username[0]}{new string('*', username.Length - 2)}{username[^1]}@{domain}";
    }

    private static string? GetUserIdFromToken(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);

            return token.Subject;
        }
        catch
        {
            return "unknown-user";
        }
    }
}
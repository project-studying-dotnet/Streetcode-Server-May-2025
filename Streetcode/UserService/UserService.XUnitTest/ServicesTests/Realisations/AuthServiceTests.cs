using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.DTO.Auth.Responses;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;
using UserService.WebApi.Services.Realisations;

namespace UserService.XUnitTest.ServicesTests.Realisations;

public class AuthServiceTests
{
    private const string ValidationFailedError = "Login validation failed";
    private const string InvalidCredentialsError = "Invalid credentials";
    private const string TokenGenerationFailedError = "Token generation failed";
    private const string TokenRefreshFailedError = "Token refresh failed";
    private const string RefreshTokenNullOrEmptyError = "Refresh token is null or empty";

    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly AuthService _authService;
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IValidator<LoginRequestDTO>> _validator = new();

    public AuthServiceTests()
    {
        _mapperMock = new();
        _loggerMock = new();
        _userManagerMock = new();
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null
        );

        _authService = new AuthService(
            _mapperMock.Object,
            _loggerMock.Object,
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _validator.Object);
    }

    [Fact]
    public async Task Register_UserByEmailExists_ReturnsFail()
    {
        // Arrange
        var newUserDto = new RegisterUserDTO { Email = "existing_kobilinskiyn@gmail.com" };
        var existingUser = new User { Email = newUserDto.Email };
        
        _userManagerMock
            .Setup(um => um.FindByEmailAsync(newUserDto.Email))
            .ReturnsAsync(existingUser);

        var errorMsg = "User with this email already exists";

        // Act
        var result = await _authService.Register(newUserDto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Select(e => e.Message).Should().Contain(errorMsg);
    }

    [Fact]
    public async Task Register_CreateUserFails_ReturnsFail()
    {
        // Arrange
        var newUserDto = new RegisterUserDTO { Email = "kobilinskiyn@gmail.com", Password = "Test123!" };
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(newUserDto.Email))
            .ReturnsAsync(null as User); 

        _mapperMock
            .Setup(m => m.Map<User>(newUserDto))
            .Returns(new User { Email = newUserDto.Email });

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<User>(), newUserDto.Password))
            .ReturnsAsync(identityResult);

        var errorMsg = "Failed to create user: Invalid password";

        // Act
        var result = await _authService.Register(newUserDto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Select(e => e.Message).Should().Contain(errorMsg);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        // Arrange
        var newUserDto = new RegisterUserDTO { Email = "validkobilinskiyn@gmail.com", Password = "Password123!" };
        var newUser = new User { Id = "id-nikita-123", Email = newUserDto.Email };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(newUserDto.Email))
            .ReturnsAsync(null as User); 

        _mapperMock
            .Setup(m => m.Map<User>(newUserDto))
            .Returns(newUser);

        _userManagerMock
            .Setup(um => um.CreateAsync(newUser, newUserDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.Register(newUserDto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(newUser);
    }

    [Fact]
    public async Task Login_ValidationFails_ReturnsFail()
    {
        // Arrange
        var dto = CreateLoginDto("bad", "");
        var validationFailure = new ValidationFailure("Email", "Required");
        _validator.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult([validationFailure]));

        // Act
        var result = await _authService.LoginAsync(dto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains(ValidationFailedError));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("kobilinskiyn@gmail.com")]
    public async Task Login_AuthenticationFails_ReturnsFail(string? email)
    {
        // Arrange
        var dto = CreateLoginDto();
        var user = email != null ? CreateUser(email: email) : null;

        _validator.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        if (user != null)
        {
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(false);
        }

        // Act
        var result = await _authService.LoginAsync(dto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message == InvalidCredentialsError);
    }

    [Fact]
    public async Task Login_TokenGenerationFails_ReturnsFail()
    {
        // Arrange
        var dto = CreateLoginDto();
        var user = CreateUser();

        SetupSuccessfulLogin(dto, user);

        _tokenServiceMock.Setup(t => t.GenerateTokensAsync(user, default))
            .ReturnsAsync(Result.Fail<TokenResponseDTO>(TokenGenerationFailedError));

        // Act
        var result = await _authService.LoginAsync(dto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message == TokenGenerationFailedError);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var dto = CreateLoginDto();
        var user = CreateUser();
        var token = CreateTokenResponse();

        SetupSuccessfulLogin(dto, user);

        _tokenServiceMock.Setup(t => t.GenerateTokensAsync(user, default))
            .ReturnsAsync(Result.Ok(token));

        // Act
        var result = await _authService.LoginAsync(dto, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task RefreshToken_InvalidToken_ReturnsFail(string? refreshToken)
    {
        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message == RefreshTokenNullOrEmptyError);
    }

    [Fact]
    public async Task RefreshToken_ServiceFails_ReturnsFail()
    {
        // Arrange
        const string token = "refresh";
        _tokenServiceMock.Setup(t => t.RefreshAccessTokenAsync(token, default))
            .ReturnsAsync(Result.Fail<TokenResponseDTO>("not found"));

        // Act
        var result = await _authService.RefreshTokenAsync(token, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message == TokenRefreshFailedError);
    }

    [Fact]
    public async Task RefreshToken_Valid_ReturnsSuccess()
    {
        // Arrange
        const string refreshToken = "refresh";
        var tokenResponse = CreateTokenResponse();

        _tokenServiceMock.Setup(t => t.RefreshAccessTokenAsync(refreshToken, default))
            .ReturnsAsync(Result.Ok(tokenResponse));

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(tokenResponse);
    }

    private void SetupSuccessfulLogin(LoginRequestDTO dto, User user)
    {
        _validator.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        _userManagerMock.Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(um => um.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);
    }

    private static LoginRequestDTO CreateLoginDto(
        string email = "kobilinskiyn@gmail.com",
        string password = "GoodPwd1!") => new()
    {
        Email = email,
        Password = password
    };

    private static User CreateUser(
        string id = "uid-42",
        string email = "user@mail.com") => new()
    {
        Id = id,
        Email = email
    };

    private static TokenResponseDTO CreateTokenResponse() => new()
    {
        AccessToken = "jwt",
        AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
        RefreshToken = "r",
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
    };
}
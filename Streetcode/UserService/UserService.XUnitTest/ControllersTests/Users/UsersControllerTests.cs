using FluentResults;
using UserService.WebApi.Controllers.Users;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.DTO.Auth.Responses;

namespace UserService.XUnitTest.ControllersTests.Users;

public class UsersControllerTests
{
    private static readonly string WrongCredentialsMsg = "wrong";
    private static readonly string RefreshExpiredMsg = "expired / revoked";
    private static readonly string[] WrongCredentialsArray = [WrongCredentialsMsg];
    private static readonly string[] RefreshExpiredArray = [RefreshExpiredMsg];

    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _mapperMock = new Mock<IMapper>();
        _controller = new UsersController(_authServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Register_ValidModel_ReturnsOkWithUserResponse()
    {
        // Arrange
        var registerDto = new RegisterUserDTO
        {
            Name = "Vika",
            Surname = "Yashan",
            Email = "yashan@gmail.com",
            Password = "*Password123"
        };

        var user = new User
        {
            Id = "1",
            Name = "Vika",
            Surname = "Yashan",
            Email = "yashan@gmail.com"
        };

        var userResponseDto = new UserResponseDTO
        {
            Id = "1",
            Name = "Vika",
            Surname = "Yashan",
            Email = "yashan@gmail.com"
        };

        var successResult = Result.Ok(user);

        _authServiceMock
            .Setup(x => x.Register(registerDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        _mapperMock
            .Setup(x => x.Map<UserResponseDTO>(user))
            .Returns(userResponseDto);

        // Act
        var result = await _controller.Register(registerDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(userResponseDto);
    }

    [Fact]
    public async Task Register_InvalidModel_ReturnsBadRequestWithModelState()
    {
        // Arrange
        var registerDto = new RegisterUserDTO();
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Register(registerDto, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().BeOfType<SerializableError>();
    }

    [Fact]
    public async Task Register_ServiceReturnsFailure_ReturnsBadRequestWithError()
    {
        // Arrange
        var registerDto = new RegisterUserDTO
        {
            Name = "Vika",
            Surname = "Yashan",
            Email = "yashan@gmail.com",
            Password = "password123"
        };

        var error = new Error("User with this email already exists");
        var failureResult = Result.Fail<User>(error);

        _authServiceMock
            .Setup(x => x.Register(registerDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Register(registerDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var errorResponse = badRequestResult!.Value;
        errorResponse.Should().BeEquivalentTo(new { Error = "User with this email already exists" });
    }

    [Fact]
    public async Task Login_ServiceSuccess_ReturnsOkWithToken()
    {
        // Arrange
        var req = new LoginRequestDTO { Email = "kobilinskiyn@gmail.com", Password = "Pwd123!" };
        var token = new TokenResponseDTO
        {
            AccessToken = "jwt",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshToken = "r",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _authServiceMock.Setup(a => a.LoginAsync(req, default)).ReturnsAsync(Result.Ok(token));

        // Act
        var result = await _controller.Login(req, default);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(token);
    }

    [Fact]
    public async Task Login_ServiceFail_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var req = new LoginRequestDTO { Email = "nikita@mail.com", Password = "bad" };
        _authServiceMock.Setup(a => a.LoginAsync(req, default))
             .ReturnsAsync(Result.Fail<TokenResponseDTO>("wrong"));

        // Act
        var result = await _controller.Login(req, default);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeEquivalentTo(new { Errors = WrongCredentialsArray });
    }

    [Fact]
    public async Task RefreshToken_ServiceSuccess_ReturnsOkWithToken()
    {
        // Arrange
        var req = new RefreshTokenRequestDTO { RefreshToken = "refresh" };
        var token = new TokenResponseDTO { AccessToken = "new-jwt", AccessTokenExpiresAt = DateTime.UtcNow };

        _authServiceMock.Setup(a => a.RefreshTokenAsync(req.RefreshToken, default))
             .ReturnsAsync(Result.Ok(token));

        // Act
        var result = await _controller.RefreshToken(req, default);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(token);
    }

    [Fact]
    public async Task RefreshToken_ServiceFail_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var req = new RefreshTokenRequestDTO { RefreshToken = "expired" };
        _authServiceMock.Setup(a => a.RefreshTokenAsync(req.RefreshToken, default))
             .ReturnsAsync(Result.Fail<TokenResponseDTO>("expired / revoked"));

        // Act
        var result = await _controller.RefreshToken(req, default);

        // Assert
        var bad = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        bad.Value.Should().BeEquivalentTo(new { Errors = RefreshExpiredArray });
    }
}
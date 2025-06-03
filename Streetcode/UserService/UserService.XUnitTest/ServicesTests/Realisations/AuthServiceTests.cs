using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Realisations;

namespace UserService.XUnitTest.ServicesTests.Realisations;

public class AuthServiceTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mapperMock = new();
        _loggerMock = new();
        _userManagerMock = new();
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null
        );

        _authService = new AuthService(_mapperMock.Object, _loggerMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Register_UserByEmailExists_ReturnsFail()
    {
        // Arrange
        var newUserDto = new RegisterUserDTO { Email = "existing_email@gmail.com" };
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
        var newUserDto = new RegisterUserDTO { Email = "newuser@example.com", Password = "Test123!" };
        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(newUserDto.Email))
            .ReturnsAsync((User)null); 

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
        var newUserDto = new RegisterUserDTO { Email = "valid@example.com", Password = "Password123!" };
        var newUser = new User { Id = "id-string-123", Email = newUserDto.Email };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(newUserDto.Email))
            .ReturnsAsync((User)null); 

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
}

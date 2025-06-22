using Moq;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.MediatR.Auth.ChangePassword;
using UserService.WebApi.Services.Interfaces;
using Xunit;
using FluentResults;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Auth.ChangePassword;
public class ChangePasswordHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = new ChangePasswordRequestDTO
        {
            Email = "test@example.com",
            OldPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService
            .Setup(s => s.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        var handler = new ChangePasswordHandler(mockAuthService.Object);

        // Act
        var result = await handler.Handle(new ChangePasswordCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockAuthService.Verify(s => s.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidOldPassword_ReturnsFailure()
    {
        // Arrange
        var dto = new ChangePasswordRequestDTO
        {
            Email = "test@example.com",
            OldPassword = "WrongOldPass",
            NewPassword = "NewPass456!"
        };

        var mockAuthService = new Mock<IAuthService>();
        mockAuthService
            .Setup(s => s.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Old password is incorrect."));

        var handler = new ChangePasswordHandler(mockAuthService.Object);

        // Act
        var result = await handler.Handle(new ChangePasswordCommand(dto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "Old password is incorrect.");
        mockAuthService.Verify(s => s.ChangePasswordAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }
}

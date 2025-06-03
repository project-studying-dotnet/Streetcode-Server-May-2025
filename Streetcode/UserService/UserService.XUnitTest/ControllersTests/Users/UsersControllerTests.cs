using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.WebApi.Controllers.Users;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.XUnitTest.ControllersTests.Users;

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class UsersControllerTests
{
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
}
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.Create;
using Streetcode.BLL.Validator.Streetcode.Text.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Text.Create;

public class CreateTextHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly CreateTextHandler _handler;

    public CreateTextHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new CreateTextHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMappedEntityIsNull()
    {
        // Arrange
        var requestDto = new TextCreateDTO();
        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns((Entity)null!);

        // Act
        var result = await _handler.Handle(new CreateTextCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Should_Fail_When_Title_Too_Long()
    {
        var validator = new CreateTextValidator();
        var command = new CreateTextCommand(new TextCreateDTO
        {
            Title = new string('A', 101),
            StreetcodeId = 1,
            TextContent = "Some text"
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.First().ErrorMessage.Should().Contain("Title cannot be more than 100 characters");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenTextIsCreated()
    {
        // Arrange
        var requestDto = new TextCreateDTO
        {
            StreetcodeId = 5,
            Title = "Valid title",
            TextContent = "Some valid text"
        };

        var mappedEntity = new Entity
        {
            StreetcodeId = 5,
            Title = requestDto.Title,
            TextContent = requestDto.TextContent
        };

        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns(mappedEntity);
        _repositoryMock.Setup(r => r.TextRepository.CreateAsync(mappedEntity)).ReturnsAsync(mappedEntity);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<TextDTO>(mappedEntity)).Returns(new TextDTO { Title = mappedEntity.Title });

        // Act
        var result = await _handler.Handle(new CreateTextCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Valid title");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenSaveChangesFails()
    {
        // Arrange
        var requestDto = new TextCreateDTO
        {
            StreetcodeId = 1,
            Title = "Title",
            TextContent = "Some text"
        };

        var mappedEntity = new Entity
        {
            StreetcodeId = 1,
            Title = requestDto.Title,
            TextContent = requestDto.TextContent
        };

        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns(mappedEntity);
        _repositoryMock.Setup(r => r.TextRepository.CreateAsync(mappedEntity)).ReturnsAsync(mappedEntity);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new CreateTextCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Failed to save new Text.");
    }
}
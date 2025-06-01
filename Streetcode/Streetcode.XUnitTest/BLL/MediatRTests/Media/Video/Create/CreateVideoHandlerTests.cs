using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Video.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Media.Video;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Video.Create;

public class CreateVideoHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly CreateVideoHandler _handler;

    public CreateVideoHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new CreateVideoHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
            );
    }

    [Fact]
    public async Task Handle_OnVideoCreation_ReturnsSuccess()
    {
        // Arrange
        var requestDto = new CreateVideoDTO
        {
            Title = "Title",
            Description = "Description",
            Url = "https://google.com",
            StreetcodeId = 1,
        };
        var mappedEntity = new Entity
        {
            Title = "Title",
            Description = "Description",
            Url = "https://google.com",
            StreetcodeId = 1,
        };
        _mapperMock
            .Setup(m => m.Map<Entity>(requestDto))
            .Returns(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(), default))
            .ReturnsAsync((Entity)null);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateVideoCommand(requestDto), CancellationToken.None);

        // Assert
        _repositoryWrapperMock.Verify(x => x.VideoRepository.CreateAsync(mappedEntity), Times.Once);
        _repositoryWrapperMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyVideo_ReturnsError()
    {
        CreateVideoDTO requestDto = null;
        Entity mappedEntity = null;
        _mapperMock
            .Setup(m => m.Map<Entity>(null))
            .Returns(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(), default))
            .ReturnsAsync((Entity)null);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateVideoCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Cannot convert null to Video.");
    }

    [Fact]
    public async Task Handle_TitleMaxLength_ReturnsError()
    {
        var requestDto = new CreateVideoDTO
        {
            Title = new string('*', 101),
            Description = "Description",
            Url = "https://google.com",
            StreetcodeId = 1,
        };
        var mappedEntity = new Entity
        {
            Title = new string('*', 101),
            Description = "Description",
            Url = "https://google.com",
            StreetcodeId = 1,
        };
        _mapperMock
            .Setup(m => m.Map<Entity>(requestDto))
            .Returns(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(), default))
            .ReturnsAsync((Entity)null);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateVideoCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Заголовок відео не може бути більше 100 символів.");
    }

    [Fact]
    public async Task Handle_EmptyUrl_ReturnsError()
    {
        var requestDto = new CreateVideoDTO
        {
            Title = "Title",
            Description = "Description",
            StreetcodeId = 1,
        };
        var mappedEntity = new Entity
        {
            Title = "Title",
            Description = "Description",
            StreetcodeId = 1,
        };
        _mapperMock
            .Setup(m => m.Map<Entity>(requestDto))
            .Returns(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(), default))
            .ReturnsAsync((Entity)null);
        _repositoryWrapperMock
            .Setup(r => r.VideoRepository.CreateAsync(mappedEntity))
            .ReturnsAsync(mappedEntity);
        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateVideoCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Посилання на відео є обов'язковим.");
    }
}

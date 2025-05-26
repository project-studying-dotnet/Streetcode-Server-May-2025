using AutoMapper;
using FluentAssertions;
using Moq;
using Repositories.Interfaces;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.Create;

public class CreateArtHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IArtRepository> _mockArtRepo;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateArtHandler _handler;

    public CreateArtHandlerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockArtRepo = new Mock<IArtRepository>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new CreateArtHandler(_mockMapper.Object, _mockRepoWrapper.Object, _mockLogger.Object);
    }
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenArtIsCreatedSuccessfully()
    {
        // Arrange
        var artDto = new ArtDTO
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1
        };
        var artEntity = new ArtEntity
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1,
            StreetcodeArts = []
        };

        var command = new CreateArtCommand(artDto);

        _mockMapper.Setup(m => m.Map<ArtEntity>(artDto)).Returns(artEntity);
        _mockMapper.Setup(m => m.Map<ArtDTO>(artEntity)).Returns(artDto);

        _mockArtRepo.Setup(r => r.CreateAsync(It.IsAny<ArtEntity>())).ReturnsAsync(artEntity);
        _mockRepoWrapper.Setup(r => r.ArtRepository).Returns(_mockArtRepo.Object);
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var handler = new CreateArtHandler(_mockMapper.Object, _mockRepoWrapper.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("title");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMappingReturnsNull()
    {
        var artDto = new ArtDTO
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1
        };
        var artEntity = new ArtEntity
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1,
            StreetcodeArts = []
        };

        var command = new CreateArtCommand(artDto);

        _mockMapper.Setup(m => m.Map<ArtEntity>(It.IsAny<ArtDTO>())).Returns((ArtEntity)null);

        var handler = new CreateArtHandler(_mockMapper.Object, _mockRepoWrapper.Object, _mockLogger.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();

        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Cannot convert null to art");

        _mockLogger.Verify(l => l.LogError(command, "Cannot convert null to art"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        var artDto = new ArtDTO
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1
        };
        var artEntity = new ArtEntity
        {
            Id = 1,
            Description = "description",
            Title = "title",
            ImageId = 1,
            StreetcodeArts = []
        };

        var command = new CreateArtCommand(artDto);

        _mockMapper.Setup(m => m.Map<ArtEntity>(artDto)).Returns(artEntity);
        _mockArtRepo.Setup(r => r.CreateAsync(It.IsAny<ArtEntity>())).ReturnsAsync(artEntity);
        _mockRepoWrapper.Setup(r => r.ArtRepository).Returns(_mockArtRepo.Object);
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var handler = new CreateArtHandler(_mockMapper.Object, _mockRepoWrapper.Object, _mockLogger.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();

        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Failed to save art.");

        _mockLogger.Verify(l => l.LogError(command, "Failed to save art."), Times.Once);
    }
}
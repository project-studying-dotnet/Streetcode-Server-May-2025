using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.Create;

public class CreateAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IBlobService> _blob = new(); 
    private readonly Mock<IMapper> _map = new();
    private readonly Mock<ILoggerService> _log = new();
    private readonly CreateAudioHandler _handler;

    public CreateAudioHandlerTests() =>
        _handler = new CreateAudioHandler(_blob.Object, _repo.Object, _map.Object, _log.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenAudioCreated()
    {
        // Arrange
        var dto = new AudioFileBaseCreateDTO { Title = "Bird", BaseFormat = "b64", Extension = "mp3" };
        var entity = new EntAudio { Title = "Bird" };
        var outDto = new AudioDTO { Id = 1, Description = "Bird-song", BlobName = "hash.mp3" };

        _blob.Setup(b => b.SaveFileInStorageAsync(dto.BaseFormat, dto.Title, dto.Extension))
            .ReturnsAsync("hash");
        _map.Setup(m => m.Map<EntAudio>(dto))
            .Returns(entity);
        _map.Setup(m => m.Map<AudioDTO>(entity))
            .Returns(outDto);
        _repo.Setup(r => r.AudioRepository.CreateAsync(entity))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateAudioCommand(dto), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(outDto);
        entity.BlobName.Should().Be("hash.mp3");
    }

    [Fact] 
    public async Task Handle_Fails_WhenSaveChangesZero()
    {
        // Arrange
        var dto = new AudioFileBaseCreateDTO { Title = "x", BaseFormat = "data", Extension = "wav" };
        var entity = new EntAudio();

        _blob.Setup(b => b.SaveFileInStorageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("h");
        _map.Setup(m => m.Map<EntAudio>(dto))
            .Returns(entity);
        _repo.Setup(r => r.AudioRepository.CreateAsync(entity))
            .ReturnsAsync(entity);
        _repo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new CreateAudioCommand(dto), default);

        // Assert
        result.IsFailed.Should().BeTrue();
        _log.Verify(l => l.LogError(It.IsAny<object?>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsBlobService_WithCorrectParameters()
    {
        // Arrange
        var dto = new AudioFileBaseCreateDTO { Title = "Sea", BaseFormat = "data", Extension = "ogg" };

        _blob.Setup(b => b.SaveFileInStorageAsync(dto.BaseFormat, dto.Title, dto.Extension))
            .ReturnsAsync("hh");
        _map.Setup(m => m.Map<EntAudio>(dto))
            .Returns(new EntAudio());
        _map.Setup(m => m.Map<AudioDTO>(It.IsAny<EntAudio>()))
            .Returns(new AudioDTO());
        _repo.Setup(r => r.AudioRepository.CreateAsync(It.IsAny<EntAudio>()))
            .ReturnsAsync(new EntAudio());
        _repo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new CreateAudioCommand(dto), default);

        // Assert
        _blob.Verify(b => b.SaveFileInStorageAsync("data", "Sea", "ogg"), Times.Once);
    }
}
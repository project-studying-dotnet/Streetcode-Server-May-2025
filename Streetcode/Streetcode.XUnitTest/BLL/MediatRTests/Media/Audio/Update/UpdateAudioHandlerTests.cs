using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.DAL.Entities.Media;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;
using Xunit;
using Repositories.Interfaces;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.Update;

public class UpdateAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IAudioRepository> _mockAudioRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly UpdateAudioHandler _handler;

    public UpdateAudioHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockAudioRepository = new Mock<IAudioRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLoggerService = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(r => r.AudioRepository).Returns(_mockAudioRepository.Object);

        _handler = new UpdateAudioHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLoggerService.Object
        );
    }

    [Fact]
    public async Task Handle_AudioNotFoundForUpdate_ReturnsFailResult()
    {
        // Arrange
        var audioDto = new AudioDTO { Id = 99, Description = "Non Existent" };
        var command = new UpdateAudioCommand(audioDto);

        _mockAudioRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync((AudioEntity)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain($"Audio with ID {audioDto.Id} not found.");
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncFailsDuringUpdate_ReturnsFailResult()
    {
        // Arrange
        var audioDto = new AudioDTO { Id = 1, Description = "Updated Title" };
        var command = new UpdateAudioCommand(audioDto);
        var existingEntity = new AudioEntity { Id = 1, Title = "Old Title", BlobName = "audio.mp3" };

        _mockAudioRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync(existingEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain($"Failed to update Audio with ID {audioDto.Id}");
    }

    [Fact]
    public async Task Handle_BlobServiceThrowsException_ThrowsException()
    {
        // Arrange
        var audioDto = new AudioDTO { Id = 1, Description = "Updated Title", BlobName = "audio.mp3" };
        var command = new UpdateAudioCommand(audioDto);

        var existingEntity = new AudioEntity { Id = 1, Title = "Old Title", BlobName = "audio.mp3" };

        _mockAudioRepository.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(), null))
            .ReturnsAsync(existingEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<AudioDTO>(It.IsAny<AudioEntity>()))
            .Returns(new AudioDTO { Id = 1, Description = "Updated Title", BlobName = "audio.mp3" });

        var expectedException = new InvalidOperationException("Failed to fetch base64 audio");
        _mockBlobService.Setup(s => s.FindFileInStorageAsBase64Async("audio.mp3"))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage(expectedException.Message);
    }
}

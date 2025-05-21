using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.GetBaseAudio;

public class GetBaseAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IBlobService> _blob = new();
    private readonly Mock<ILoggerService> _log = new(); 
    private readonly GetBaseAudioHandler _handler;

    public GetBaseAudioHandlerTests() =>
        _handler = new GetBaseAudioHandler(_blob.Object, _repo.Object, _log.Object);

    [Fact]
    public async Task Handle_WhenAudioExists_ReturnsSuccessResult()
    {
        // Arrange
        var audio = new EntAudio { Id = 7, BlobName = "x.mp3" };
        var stream = new MemoryStream(new byte[] { 1 });
        SetupRepository(audio);
        SetupBlobService("x.mp3", stream);

        // Act
        var result = await _handler.Handle(new GetBaseAudioQuery(7), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAudioNotFound_ReturnsFailureResult()
    {
        // Arrange
        SetupRepository(null);

        // Act
        var result = await _handler.Handle(new GetBaseAudioQuery(99), default);

        // Assert 
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CallsBlobServiceOnce()
    {
        // Arrange
        var audio = new EntAudio { Id = 1, BlobName = "y.mp3" };
        SetupRepository(audio);
        SetupBlobService("y.mp3", new MemoryStream());

        // Act
        await _handler.Handle(new GetBaseAudioQuery(1), default);

        // Assert
        _blob.Verify(b => b.FindFileInStorageAsMemoryStreamAsync("y.mp3"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_DoesNotLogError()
    {
        // Arrange
        var audio = new EntAudio { Id = 2, BlobName = "z.mp3" };
        SetupRepository(audio);
        SetupBlobService("z.mp3", new MemoryStream());

        // Act
        await _handler.Handle(new GetBaseAudioQuery(2), default);

        // Assert
        _log.Verify(l => l.LogError(It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    private void SetupRepository(EntAudio? audio)
    {
        _repo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<EntAudio, bool>>>(),
                It.IsAny<Func<IQueryable<EntAudio>, IIncludableQueryable<EntAudio, object>>>()))
            .ReturnsAsync(audio);
    }

    private void SetupBlobService(string blobName, MemoryStream stream)
    {
        _blob.Setup(b => b.FindFileInStorageAsMemoryStreamAsync(blobName))
            .ReturnsAsync(stream);
    }
}
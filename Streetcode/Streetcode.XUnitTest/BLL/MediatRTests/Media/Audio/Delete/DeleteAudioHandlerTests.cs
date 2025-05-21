using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.Delete;

public class DeleteAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IBlobService> _blob = new();
    private readonly Mock<ILoggerService> _log = new();
    private readonly DeleteAudioHandler _handler;

    public DeleteAudioHandlerTests() =>
        _handler = new DeleteAudioHandler(_repo.Object, _blob.Object, _log.Object);

    [Fact]
    public async Task Handle_Success_DeletesBlob()
    {
        // Arrange
        var audio = new EntAudio { Id = 3, BlobName = "f.mp3" };
        SetupGetAudio(audio);
        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteAudioCommand(3), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _blob.Verify(b => b.DeleteFileInStorageAsync("f.mp3"), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ReturnsFailed()
    {
        // Arrange
        SetupGetAudio(null);

        // Act
        var result = await _handler.Handle(new DeleteAudioCommand(99), default);

        // Assert
        result.IsFailed.Should().BeTrue();
        VerifyBlobNotDeleted();
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsFailed()
    {
        // Arrange
        var audio = new EntAudio { Id = 4, BlobName = "g.mp3" };
        SetupGetAudio(audio);
        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new DeleteAudioCommand(4), default);

        // Assert
        result.IsFailed.Should().BeTrue();
        VerifyBlobNotDeleted();
    }

    [Fact]
    public async Task Handle_Success_DeletesFromRepo()
    {
        // Arrange
        var audio = new EntAudio { Id = 5, BlobName = "h.mp3" };
        SetupGetAudio(audio);
        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(new DeleteAudioCommand(5), default);

        // Assert
        _repo.Verify(r => r.AudioRepository.Delete(audio), Times.Once);
    }

    private void SetupGetAudio(EntAudio? audio) =>
        _repo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<EntAudio, bool>>>(),
                It.IsAny<Func<IQueryable<EntAudio>, IIncludableQueryable<EntAudio, object>>>()))
            .ReturnsAsync(audio);

    private void VerifyBlobNotDeleted() =>
        _blob.Verify(b => b.DeleteFileInStorageAsync(It.IsAny<string>()), Times.Never);
}
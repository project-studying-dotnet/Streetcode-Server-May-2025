using Xunit;
using Moq;
using Repositories.Interfaces;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Delete;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.Delete;

public class DeleteArtHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IArtRepository> _artRepoMock;
    private readonly Mock<IBlobService> _blobServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly DeleteArtHandler _handler;

    public DeleteArtHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _artRepoMock = new Mock<IArtRepository>();
        _blobServiceMock = new Mock<IBlobService>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock.Setup(r => r.ArtRepository).Returns(_artRepoMock.Object);
        _handler = new DeleteArtHandler(_repositoryWrapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ArtNotFound_ReturnsFailResult()
    {
        // Arrange
        int artId = 1;
        _artRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArtEntity, bool>>>(), default))
                    .ReturnsAsync((ArtEntity)null);

        // Act
        var result = await _handler.Handle(new DeleteArtCommand(artId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        _loggerMock.Verify(l => l.LogError(It.IsAny<DeleteArtCommand>(), It.Is<string>(s => s.Contains("Cannot find an art"))), Times.Once);
    }

    [Fact]
    public async Task Handle_SuccessfulDeleteNoImageBlob_LogsWarningAndReturnsSuccess()
    {
        // Arrange
        var artId = 1;
        var art = new ArtEntity
        {
            Id = artId,
            Image = null
        };

        _artRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
            .ReturnsAsync(art);

        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var command = new DeleteArtCommand(artId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _artRepoMock.Verify(r => r.Delete(art), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _blobServiceMock.Verify(b => b.DeleteFileInStorageAsync(It.IsAny<string>()), Times.Never);

        _loggerMock.Verify(
            l => l.LogWarning(It.Is<string>(s => s == $"Art ID: {artId} did not have an associated image blob name to delete.")),
            Times.Once);
        _loggerMock.Verify(
            l => l.LogInformation(It.Is<string>(s => s == $"DeleteArtCommand for Art ID: {artId} handled successfully (database entity deleted).")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SuccessfulDbDeleteButBlobDeleteFails_LogsErrorForBlobButReturnsSuccess()
    {
        // Arrange
        var artId = 1;
        var blobName = "test_image.png";
        var art = new ArtEntity
        {
            Id = artId,
            Image = new ImageEntity { BlobName = blobName }
        };
        var blobExceptionMessage = "Blob service unavailable";

        _artRepoMock.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
            .ReturnsAsync(art);

        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _blobServiceMock.Setup(b => b.DeleteFileInStorageAsync(blobName))
            .ThrowsAsync(new Exception(blobExceptionMessage));

        var command = new DeleteArtCommand(artId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _artRepoMock.Verify(r => r.Delete(art), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _blobServiceMock.Verify(b => b.DeleteFileInStorageAsync(blobName), Times.Once);

        _loggerMock.Verify(
            l => l.LogError(command, It.Is<string>(s => s == $"Failed to delete blob: {blobName} for Art ID: {artId}. Error: {blobExceptionMessage}")),
            Times.Once);
        _loggerMock.Verify(
            l => l.LogInformation(It.Is<string>(s => s == $"DeleteArtCommand for Art ID: {artId} handled successfully (database entity deleted).")),
            Times.Once);
        _loggerMock.Verify(
            l => l.LogInformation(It.Is<string>(s => s.Contains("Successfully deleted blob"))),
            Times.Never);
    }
}
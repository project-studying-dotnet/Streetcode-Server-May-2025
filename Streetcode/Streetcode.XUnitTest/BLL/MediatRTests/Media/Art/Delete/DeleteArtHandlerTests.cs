using Xunit;
using Moq;
using Repositories.Interfaces;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Delete;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

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
    public async Task Handle_DeleteSuccessful_DeletesFileAndReturnsSuccess()
    {
        // Arrange
        var art = new ArtEntity { Id = 1, Title = "art1" };
        _artRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArtEntity, bool>>>(), default))
                    .ReturnsAsync(art);

        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                 .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteArtCommand(art.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _artRepoMock.Verify(r => r.Delete(art), Times.Once);
        _blobServiceMock.Verify(b => b.DeleteFileInStorageAsync(art.Title), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("handled successfully"))), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteFails_LogsErrorAndReturnsFail()
    {
        // Arrange
        var art = new ArtEntity { Id = 1, Title = "art1" };
        _artRepoMock.Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ArtEntity, bool>>>(), default))
                    .ReturnsAsync(art);

        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                 .ReturnsAsync(0); // Збереження неуспішне

        // Act
        var result = await _handler.Handle(new DeleteArtCommand(art.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        _artRepoMock.Verify(r => r.Delete(art), Times.Once);
        _blobServiceMock.Verify(b => b.DeleteFileInStorageAsync(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.LogError(It.IsAny<DeleteArtCommand>(), It.Is<string>(s => s.Contains("Failed to delete"))), Times.Once);
    }
}
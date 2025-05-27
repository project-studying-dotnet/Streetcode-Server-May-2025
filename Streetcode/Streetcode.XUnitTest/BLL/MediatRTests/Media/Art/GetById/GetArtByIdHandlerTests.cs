using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.GetById;

public class GetArtByIdHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetArtByIdHandler _handler;

    public GetArtByIdHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetArtByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnArt_WhenArtExists()
    {
        // Arrange
        var art = new ArtEntity { Id = 1 };
        var artDto = new ArtDTO { Id = 1 };

        _repositoryWrapperMock
            .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
            .ReturnsAsync(art);

        _mapperMock
            .Setup(m => m.Map<ArtDTO>(
                It.IsAny<ArtEntity>()))
            .Returns(artDto);

        var query = new GetArtByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(artDto, result.Value);
        _mapperMock.Verify(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenArtIsNull()
    {
        // Arrange
        _repositoryWrapperMock
           .Setup(r => r.ArtRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<ArtEntity, bool>>>(),
               It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
           .ReturnsAsync((ArtEntity)null);

        var query = new GetArtByIdQuery(1);
        var expectedMessage = $"Cannot find an art with corresponding id: {query.Id}";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedMessage, result.Errors.First().Message);
        _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        _mapperMock.Verify(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()), Times.Never);
    }
}
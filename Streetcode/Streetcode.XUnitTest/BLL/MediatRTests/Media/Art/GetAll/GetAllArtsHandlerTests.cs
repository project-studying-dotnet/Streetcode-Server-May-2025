using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.GetAll
{
    public class GetAllArtsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _mockBlobService;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllArtsHandler _handler;

        public GetAllArtsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _mockBlobService = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllArtsHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _mockBlobService.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnArts_WhenArtsExist()
        {
            // Arrange
            var arts = new List<ArtEntity> { new ArtEntity { Id = 1 }, new ArtEntity { Id = 2 } };
            var artsDto = new List<ArtDTO> { new ArtDTO { Id = 1 }, new ArtDTO { Id = 2 } };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(arts);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(
                    It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(artsDto);

            var query = new GetAllArtsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(artsDto, result.Value);
            _mapperMock.Verify(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenArtsListIsEmpty()
        {
            // Arrange
            var emptyList = new List<ArtEntity>();
            var emptyDtos = new List<ArtDTO>();

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(emptyList);

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(emptyList))
                .Returns(emptyDtos);

            var query = new GetAllArtsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenArtsListIsNull()
        {
            // Arrange
            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((List<ArtEntity>)null);

            var query = new GetAllArtsQuery();
            var expectedMessage = "Cannot find any arts";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(new GetAllArtsQuery(), expectedMessage), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()), Times.Never);
        }
    }
}

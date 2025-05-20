using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.GetByStreetcodeId
{
    public class GetArtsByStreetcodeIdHandlerTests
    {

        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IBlobService> _mockBlobService;
        private readonly GetArtsByStreetcodeIdHandler _handler;

        public GetArtsByStreetcodeIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mockBlobService = new Mock<IBlobService>();
            _handler = new GetArtsByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _mockBlobService.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnArts_WhenArtsExist()
        {
            // Arrange
            var image = new Image { Id = 1, BlobName = "Blob1.jpeg" };
            var imageDto = new ImageDTO { Id = 1, BlobName = image.BlobName, Base64 = "base64-image" };

            var art = new ArtEntity { Id = 1, Image = image, ImageId = image.Id };
            var artDto = new ArtDTO { Id = 1, Image = imageDto, ImageId = imageDto.Id };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            _mockBlobService
                .Setup(b => b.FindFileInStorageAsBase64(image.BlobName))
                .Returns("base64-image");


            var query = new GetArtsByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var returnedArt = Assert.Single(result.Value);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            Assert.Equal("base64-image", returnedArt.Image.Base64);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessSkippingBlobLoading_WhenImageIsNull()
        {
            // Arrange
            var art = new ArtEntity { Id = 1, Image = null, ImageId = 0 }; 
            var artDto = new ArtDTO { Id = 1, Image = null, ImageId = 0 };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            var query = new GetArtsByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            _mockBlobService.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessSkippingBlobLoading_WhenImageBlobNameIsNull()
        {
            // Arrange
            var art = new ArtEntity { Id = 1, Image = new Image { Id = 2, BlobName = null}, ImageId = 2 };
            var artDto = new ArtDTO { Id = 1, Image = new ImageDTO { Id = 2, BlobName = null }, ImageId = 2 };

            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity> { art });

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO> { artDto });

            var query = new GetArtsByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new List<ArtDTO> { artDto }, result.Value);
            _mockBlobService.Verify(b => b.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenArtsListIsEmpty()
        {
            // Arrange
            _repositoryWrapperMock
                .Setup(r => r.ArtRepository.GetAllAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(new List<ArtEntity>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<ArtEntity>>()))
                .Returns(new List<ArtDTO>());

            var query = new GetArtsByStreetcodeIdQuery(1);

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

            var query = new GetArtsByStreetcodeIdQuery(1);
            var expectedMessage = $"Cannot find any art with corresponding streetcode id: {query.StreetcodeId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        }
    }
}

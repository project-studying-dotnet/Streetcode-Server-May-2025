using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;
using System.Linq.Expressions;
using FluentAssertions;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.SourceLinkCategory.GetCategoriesByStreetcodeId
{
    public class GetCategoriesByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetCategoriesByStreetcodeIdHandler _handler;

        public GetCategoriesByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();

            _handler = new GetCategoriesByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCategoriesExist()
        {
            // Arrange
            var streetcodeId = 1;
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>
            {
                new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 1, Title = "Category1", Image = new global::Streetcode.DAL.Entities.Media.Images.Image { BlobName = "blob1" } },
                new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 2, Title = "Category2", Image = new global::Streetcode.DAL.Entities.Media.Images.Image { BlobName = "blob2" } }
            };

            var categoryDTOs = new List<global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO>
            {
                new global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO { Id = 1, Title = "Category1", Image = new global::Streetcode.BLL.DTO.Media.Images.ImageDTO { BlobName = "blob1" } },
                new global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO { Id = 2, Title = "Category2", Image = new global::Streetcode.BLL.DTO.Media.Images.ImageDTO { BlobName = "blob2" } }
            };

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO>>(categories))
                .Returns(categoryDTOs);

            _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64Async("blob1")).ReturnsAsync("base64image1");
            _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64Async("blob2")).ReturnsAsync("base64image2");

            // Act
            var result = await _handler.Handle(new GetCategoriesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(categoryDTOs);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenNoCategoriesExist()
        {
            // Arrange
            var streetcodeId = 1;
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>();
            var categoryDTOs = new List<global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO>();

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO>>(categories))
                .Returns(categoryDTOs);

            // Act
            var result = await _handler.Handle(new GetCategoriesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            var streetcodeId = 1;

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetCategoriesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<GetCategoriesByStreetcodeIdQuery>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}

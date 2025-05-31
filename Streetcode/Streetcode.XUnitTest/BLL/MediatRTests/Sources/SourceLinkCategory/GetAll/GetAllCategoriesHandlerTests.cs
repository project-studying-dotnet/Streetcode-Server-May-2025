using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.BLL.DTO.Sources;
using FluentAssertions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.SourceLinkCategory.GetAll
{
    public class GetAllCategoriesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetAllCategoriesHandler _handler;

        public GetAllCategoriesHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();

            _handler = new GetAllCategoriesHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory> { new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 1, Title = "Category1", Image = new global::Streetcode.DAL.Entities.Media.Images.Image { BlobName = "blob1" } } };
            var categoryDTOs = new List<SourceLinkCategoryDTO> { new SourceLinkCategoryDTO { Id = 1, Title = "Category1", Image = new ImageDTO { BlobName = "blob1" } } };

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(null, It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories)).Returns(categoryDTOs);
            _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64Async("blob1")).ReturnsAsync("base64image");

            // Act
            var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(categoryDTOs);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenNoCategoriesExist()
        {
            // Arrange
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>();
            var categoryDTOs = new List<SourceLinkCategoryDTO>();

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(null, It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<IEnumerable<SourceLinkCategoryDTO>>(categories)).Returns(categoryDTOs);

            // Act
            var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(null, It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}

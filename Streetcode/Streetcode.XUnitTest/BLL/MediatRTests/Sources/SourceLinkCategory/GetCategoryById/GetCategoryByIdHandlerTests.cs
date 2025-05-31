using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;
using Streetcode.BLL.DTO.Sources;
using FluentAssertions;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.SourceLinkCategory.GetCategoryById
{
    public class GetCategoryByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetCategoryByIdHandler _handler;

        public GetCategoryByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();

            _handler = new GetCategoryByIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _blobServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCategoryExists()
        {
            // Arrange
            var categoryEntity = new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 1, Title = "Category1", Image = new global::Streetcode.DAL.Entities.Media.Images.Image { BlobName = "blob1" } };
            var categoryDto = new global::Streetcode.BLL.DTO.Sources.SourceLinkCategoryDTO { Id = 1, Title = "Category1", Image = new global::Streetcode.BLL.DTO.Media.Images.ImageDTO { BlobName = "blob1" } };
            var query = new GetCategoryByIdQuery(1);

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(), It.IsAny<System.Func<System.Linq.IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ReturnsAsync(categoryEntity);
            _mapperMock.Setup(m => m.Map<SourceLinkCategoryDTO>(categoryEntity)).Returns(categoryDto);
            _blobServiceMock.Setup(b => b.FindFileInStorageAsBase64Async("blob1")).ReturnsAsync("base64image");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(categoryDto);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenCategoryDoesNotExist()
        {
            // Arrange
            var query = new GetCategoryByIdQuery(1);
            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(), It.IsAny<System.Func<System.Linq.IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ReturnsAsync((global::Streetcode.DAL.Entities.Sources.SourceLinkCategory)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(query, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            var query = new GetCategoryByIdQuery(1);
            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(), It.IsAny<System.Func<System.Linq.IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>())).ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(query, It.IsAny<string>()),
                Times.Once);
        }
    }
}

using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.BLL.DTO.Sources;
using System.Linq.Expressions;
using FluentAssertions;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.SourceLinkCategory.GetAll
{
    public class GetAllCategoryNamesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetAllCategoryNamesHandler _handler;

        public GetAllCategoryNamesHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();

            _handler = new GetAllCategoryNamesHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>
            {
                new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 1, Title = "Category1" },
                new global::Streetcode.DAL.Entities.Sources.SourceLinkCategory { Id = 2, Title = "Category2" }
            };

            var categoryDTOs = new List<CategoryWithNameDTO>
            {
                new CategoryWithNameDTO { Id = 1, Title = "Category1" },
                new CategoryWithNameDTO { Id = 2, Title = "Category2" }
            };

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryWithNameDTO>>(categories))
                .Returns(categoryDTOs);

            // Act
            var result = await _handler.Handle(new GetAllCategoryNamesQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(categoryDTOs);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenNoCategoriesExist()
        {
            // Arrange
            var categories = new List<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>();
            var categoryDTOs = new List<CategoryWithNameDTO>();

            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ReturnsAsync(categories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryWithNameDTO>>(categories))
                .Returns(categoryDTOs);

            // Act
            var result = await _handler.Handle(new GetAllCategoryNamesQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            _repositoryWrapperMock.Setup(r => r.SourceCategoryRepository.GetAllAsync(
                It.IsAny<Expression<Func<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<global::Streetcode.DAL.Entities.Sources.SourceLinkCategory, object>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetAllCategoryNamesQuery(), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<GetAllCategoryNamesQuery>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}

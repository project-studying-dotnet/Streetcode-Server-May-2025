using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId;
using Streetcode.BLL.DTO.Sources;
using System.Linq.Expressions;
using FluentAssertions;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId
{
    public class GetCategoryContentByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetCategoryContentByStreetcodeIdHandler _handler;

        public GetCategoryContentByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new GetCategoryContentByStreetcodeIdHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenContentExists()
        {
            // Arrange
            var streetcodeId = 1;
            var categoryId = 1;
            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);
            var streetcodeEntity = new StreetcodeContent { Id = streetcodeId };
            var streetcodeCategoryContentEntity = new DAL.Entities.Sources.StreetcodeCategoryContent { StreetcodeId = streetcodeId, SourceLinkCategoryId = categoryId, Text = "Test content" };
            var expectedDto = new StreetcodeCategoryContentDTO { Text = "Test content", StreetcodeId = streetcodeId, SourceLinkCategoryId = categoryId };

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(streetcodeEntity);
            _repositoryWrapperMock.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync(streetcodeCategoryContentEntity);
            _mapperMock.Setup(m => m.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContentEntity)).Returns(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenStreetcodeNotFound()
        {
            // Arrange
            var streetcodeId = 1;
            var categoryId = 1;
            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync((StreetcodeContent)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            _loggerMock.Verify(
                x => x.LogError(query, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenNoContentFound()
        {
            // Arrange
            var streetcodeId = 1;
            var categoryId = 1;
            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);
            var streetcodeEntity = new StreetcodeContent { Id = streetcodeId };

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(streetcodeEntity);
            _repositoryWrapperMock.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

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
            var streetcodeId = 1;
            var categoryId = 1;
            var query = new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ThrowsAsync(new System.Exception("Database error"));

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

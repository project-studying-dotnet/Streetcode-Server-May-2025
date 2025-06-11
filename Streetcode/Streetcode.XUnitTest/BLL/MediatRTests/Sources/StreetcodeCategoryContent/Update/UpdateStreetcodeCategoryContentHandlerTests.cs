using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.StreetcodeCategoryContent.Update;
public class UpdateStreetcodeCategoryContentHandlerTests
{
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IRepositoryWrapper> _mockRepository = new();
    private readonly Mock<ILoggerService> _mockLogger = new();
    private readonly UpdateStreetcodeCategoryContentHandler _handler;

    public UpdateStreetcodeCategoryContentHandlerTests()
    {
        _handler = new UpdateStreetcodeCategoryContentHandler(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_GivenValidUpdateDto_ReturnsUpdatedDto()
    {
        // Arrange
        var updateDto = new CategoryContentUpdateDTO
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 2,
            Text = "Updated text"
        };

        var existingEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
            Text = "Old text"
        };

        var updatedEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 2,
            Text = "Updated text"
        };

        var expectedDto = new StreetcodeCategoryContentDTO
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 2,
            Text = "Updated text"
        };

        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync(existingEntity);

        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StreetcodeCategoryContentDTO>(It.IsAny<DAL.Entities.Sources.StreetcodeCategoryContent>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(updateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        _mockRepository.Verify(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null), Times.Once);

        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(It.IsAny<DAL.Entities.Sources.StreetcodeCategoryContent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryContentNotFound_ReturnsErrorMessage()
    {
        // Arrange
        var inputDto = new CategoryContentUpdateDTO
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
            Text = "Updated Text"
        };

        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);

        // Act
        var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(inputDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "CategoryContent not found");
    }

    [Fact]
    public async Task Handle_UpdateFailed_ReturnsErrorMessage()
    {
        // Arrange
        var inputDto = new CategoryContentUpdateDTO
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
            Text = "Updated Text"
        };

        var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
            Text = "Old Text"
        };

        var updatedEntity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            Id = 1,
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
            Text = "Updated Text"
        };

        _mockMapper.Setup(m =>
                m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(inputDto))
            .Returns(updatedEntity);

        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync(entity);

        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.Update(updatedEntity));
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new UpdateStreetcodeCategoryContentCommand(inputDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Update failed");
    }
}
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.DTO.Sources;
using System.Linq.Expressions;

using StreetcodeCategoryContentEntity = Streetcode.DAL.Entities.Sources.StreetcodeCategoryContent;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.StreetcodeCategoryContent.Delete;
public class DeleteStreetcodeCategoryContentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DeleteStreetcodeCategoryContentHandler _handler;
    private readonly Mock<ILoggerService> _loggerMock;

    public DeleteStreetcodeCategoryContentHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new DeleteStreetcodeCategoryContentHandler(
            _loggerMock.Object,
            _mapperMock.Object,
            _repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityExistsAndDeleted()
    {
        // Arrange
        var entity = CreateEntity(1);
        var dto = CreateDto();
        SetupMocks(entity, 1, dto);

        var command = new DeleteStreetcodeCategoryContentCommand(entity.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dto);
        VerifyMocksCalled(entity);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityDoesNotExist()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                null))
            .ReturnsAsync((StreetcodeCategoryContentEntity?)null);

        var command = new DeleteStreetcodeCategoryContentCommand(123);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        // Arrange
        var entity = CreateEntity(2);
        SetupMocks(entity, 0, null);

        var command = new DeleteStreetcodeCategoryContentCommand(entity.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Failed to delete");
        VerifyMocksCalled(entity, verifyMapping: false);
    }

    private static StreetcodeCategoryContentEntity CreateEntity(int id) =>
        new()
        {
            Id = id,
            Text = "Text",
            SourceLinkCategoryId = 1,
            StreetcodeId = 1
        };

    private static StreetcodeCategoryContentDTO CreateDto() =>
        new()
        {
            Text = "Text",
            SourceLinkCategoryId = 1,
            StreetcodeId = 1
        };

    private void SetupMocks(StreetcodeCategoryContentEntity entity, int saveResult, StreetcodeCategoryContentDTO? dto)
    {
        _repositoryWrapperMock
            .Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCategoryContentEntity, bool>>>(),
                null))
            .ReturnsAsync(entity);

        _repositoryWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(saveResult);

        if (dto != null)
        {
            _mapperMock
                .Setup(m => m.Map<StreetcodeCategoryContentDTO>(entity))
                .Returns(dto);
        }
    }

    private void VerifyMocksCalled(StreetcodeCategoryContentEntity entity, bool verifyMapping = true)
    {
        _repositoryWrapperMock.Verify(r =>
            r.StreetcodeCategoryContentRepository.Delete(entity), Times.Once);

        _repositoryWrapperMock.Verify(r =>
            r.SaveChangesAsync(), Times.Once);

        if (verifyMapping)
        {
            _mapperMock.Verify(m =>
                m.Map<StreetcodeCategoryContentDTO>(entity), Times.Once);
        }
    }
}
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Text.GetById;

public class GetTextByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetTextByIdHandler _handler;

    public GetTextByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetTextByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenTextExists()
    {
        // Arrange
        var (entity, mappedDto, targetTextId) = CreateValidTextEntityAndDto();
        SetupMocksForText(entity, mappedDto);

        // Act
        var result = await _handler.Handle(new GetTextByIdQuery(targetTextId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnTextById_WhenTextExists()
    {
        // Arrange
        var (entity, mappedDto, targetTextId) = CreateValidTextEntityAndDto();

        SetupMocksForText(entity, mappedDto);

        // Act
        var result = await _handler.Handle(new GetTextByIdQuery(targetTextId), CancellationToken.None);

        // Assert
        Assert.Equal(mappedDto, result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailedResult_WhenTextDoesNotExist()
    {
        // Arrange
        var (entities, mappedDtos, targetTextId) = CreateNullTextEntityAndDto();

        SetupMocksForText(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetTextByIdQuery(targetTextId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);

        VerifyMocksCalledOnce(true);
    }

    private static (TextEntity, TextDTO, int) CreateValidTextEntityAndDto()
    {
        const int targetTextId = 0;

        var entity = new TextEntity { Id = targetTextId, TextContent = "nikita" };
        var mappedDto = new TextDTO { Id = targetTextId, TextContent = "nikita" };

        return (entity, mappedDto, targetTextId);
    }

    private static (TextEntity?, TextDTO?, int) CreateNullTextEntityAndDto()
    {
        const int targetTextId = 0;

        TextEntity? entity = null;
        TextDTO? mappedDto = null;

        return (entity, mappedDto, targetTextId);
    }

    private void SetupMocksForText(TextEntity? entity, TextDTO? mappedDto)
    {
        _repositoryWrapperMock
            .Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
            .ReturnsAsync(entity);

        _mapperMock
            .Setup(mapper => mapper.Map<TextDTO>(It.IsAny<TextEntity>()))
            .Returns(mappedDto!);
    }

    private void VerifyMocksCalledOnce(bool verifyMapping = true)
    {
        _repositoryWrapperMock.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()),
            Times.Once);

        if (verifyMapping)
        {
            _mapperMock.Verify(mapper =>
                    mapper.Map<TextDTO>(It.IsAny<TextEntity>()),
                Times.Once);
        }
    }
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using Moq;
using Xunit;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Text.GetAll;

public class GetAllTextsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetAllTextsHandler _handler;

    public GetAllTextsHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetAllTextsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenTextsExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateValidTextEntitiesAndDtos();
        SetupMocksForTexts(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTexts_WhenTextsExist()
    {
        // Arrange
        var (entities, mappedDtosEnumerable) = CreateValidTextEntitiesAndDtos();

        var mappedDtos = mappedDtosEnumerable.ToList();

        SetupMocksForTexts(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(mappedDtos, result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenTextsNotExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateEmptyTextEntitiesAndDtos();

        SetupMocksForTexts(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenTextsNotExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateEmptyTextEntitiesAndDtos();

        SetupMocksForTexts(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result.Value);

        VerifyMocksCalledOnce();
    }

    private static (IEnumerable<TextEntity>, IEnumerable<TextDTO>) CreateValidTextEntitiesAndDtos()
    {
        var entities = new List<TextEntity>
        {
            new() { Id = 1, TextContent = "nikita" },
            new() { Id = 2, TextContent = "kobylynskyi" }
        };
        var mappedDtos = new List<TextDTO>
        {
            new() { Id = 1, TextContent = "nikita" },
            new() { Id = 2, TextContent = "kobylynskyi" }
        };

        return (entities, mappedDtos);
    }

    private static (IEnumerable<TextEntity>, IEnumerable<TextDTO>) CreateEmptyTextEntitiesAndDtos()
    {
        var entities = new List<TextEntity>();
        var mappedDtos = new List<TextDTO>();

        return (entities, mappedDtos);
    }

    private void SetupMocksForTexts(IEnumerable<TextEntity> entities, IEnumerable<TextDTO> mappedDtos)
    {
        _repositoryWrapperMock
            .Setup(repo => repo.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()))
            .ReturnsAsync(entities);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<TextDTO>>(It.IsAny<IEnumerable<TextEntity>>()))
            .Returns(mappedDtos);
    }

    private void VerifyMocksCalledOnce()
    {
        _repositoryWrapperMock.Verify(repo => repo.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()),
            Times.Once);

        _mapperMock.Verify(mapper =>
                mapper.Map<IEnumerable<TextDTO>>(It.IsAny<IEnumerable<TextEntity>>()),
            Times.Once);
    }
}
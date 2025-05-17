using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;
using StreetcodeContentEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Text.GetByStreetcodeId;

public class GetTextByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITextService> _textServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetTextByStreetcodeIdHandler _handler;

    public GetTextByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _textServiceMock = new Mock<ITextService>();
        _handler = new GetTextByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _textServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenTextExists()
    {
        // Arrange
        var (entity, mappedDto, targetStreetcodeId) = CreateValidTextEntityAndDto();
        SetupMocksForText(entity, mappedDto);

        // Act
        var result = await _handler.Handle(new GetTextByStreetcodeIdQuery(targetStreetcodeId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnTextById_WhenTextExists()
    {
        // Arrange
        var (entity, mappedDto, targetStreetcodeId) = CreateValidTextEntityAndDto();

        SetupMocksForText(entity, mappedDto);

        // Act
        var result = await _handler.Handle(new GetTextByStreetcodeIdQuery(targetStreetcodeId), CancellationToken.None);

        // Assert
        Assert.Equal(mappedDto, result.Value);

        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailedResult_WhenTextDoesNotExist()
    {
        // Arrange
        var (textEntity, textMappedDto, targetStreetcodeId) = CreateNullTextEntityAndDto();
        var streetcodeContentEntity = CreateStreetcodeContentEntity(targetStreetcodeId);

        SetupMocksForText(textEntity, textMappedDto);
        SetupMocksForStreetcode(streetcodeContentEntity);

        // Act
        var result = await _handler.Handle(new GetTextByStreetcodeIdQuery(targetStreetcodeId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        VerifyMocksCalledOnce(false);
    }

    private static (TextEntity, TextDTO, int) CreateValidTextEntityAndDto()
    {
        const int targetStreetcodeId = 0;

        var textEntity = new TextEntity { Id = 0, TextContent = "nikita", StreetcodeId = targetStreetcodeId };
        var textMappedDto = new TextDTO { Id = 1, TextContent = "nikita", StreetcodeId = targetStreetcodeId };

        return (textEntity, textMappedDto, targetStreetcodeId);
    }

    private static (TextEntity?, TextDTO?, int) CreateNullTextEntityAndDto()
    {
        const int targetStreetcodeId = 0;

        TextEntity? entity = null;
        TextDTO? mappedDto = null;

        return (entity, mappedDto, targetStreetcodeId);
    }

    private static StreetcodeContentEntity CreateStreetcodeContentEntity(int targetStreetcodeId)
    {
        var streetcodeContentEntity = new StreetcodeContentEntity { Id = targetStreetcodeId };

        return streetcodeContentEntity;
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

    private void SetupMocksForStreetcode(StreetcodeContentEntity streetcodeContentEntity)
    {
        _repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContentEntity>,
                    IIncludableQueryable<StreetcodeContentEntity, object>>>()))
            .ReturnsAsync(streetcodeContentEntity);
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
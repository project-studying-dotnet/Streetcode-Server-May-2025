using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.MediatR.Toponyms.GetById;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Toponyms.GetById;

public class GetToponymByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILoggerService> _loggerMock = new();
    private readonly GetToponymByIdHandler _handler;

    public GetToponymByIdHandlerTests()
    {
        _handler = new GetToponymByIdHandler(
            _repositoryMock.Object,
            _mapperMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenToponymExists()
    {
        // Arrange
        var (entity, mappedDto) = CreateValidToponym();
        SetupMocksForSuccessfulQuery(entity, mappedDto);

        var query = new GetToponymByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mappedDto);
        VerifyMocksCalledOnce();
    }

    [Fact] 
    public async Task Handle_ShouldReturnFailResult_WhenToponymDoesNotExist()
    {
        // Arrange
        SetupMocksForFailedQuery();
        var query = new GetToponymByIdQuery(999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        VerifyLoggingOnFailure(query);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResult_WhenIdIsNegative() 
    {
        // Arrange
        SetupMocksForFailedQuery();
        var query = new GetToponymByIdQuery(-1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        VerifyLoggingOnFailure(query);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResult_WhenIdIsZero()
    {
        // Arrange  
        SetupMocksForFailedQuery();
        var query = new GetToponymByIdQuery(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        VerifyLoggingOnFailure(query);
    }

    [Fact]
    public async Task Handle_ShouldMapCorrectly_WhenToponymFound()
    {
        // Arrange
        var (entity, mappedDto) = CreateValidToponym();
        SetupMocksForSuccessfulQuery(entity, mappedDto);

        var query = new GetToponymByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Id.Should().Be(entity.Id);
        result.Value.StreetName.Should().Be(entity.StreetName);
        _mapperMock.Verify(m => m.Map<ToponymDTO>(entity), Times.Once);
    }

    private static (Toponym entity, ToponymDTO dto) CreateValidToponym()
    {
        var entity = new Toponym { Id = 1, StreetName = "Main" };
        var dto = new ToponymDTO { Id = 1, StreetName = "Main" };
        return (entity, dto);
    }

    private void SetupMocksForSuccessfulQuery(Toponym entity, ToponymDTO mappedDto)
    {
        _repositoryMock
            .Setup(r => r.ToponymRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Toponym, bool>>>(),
                It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
            .ReturnsAsync(entity);

        _mapperMock
            .Setup(m => m.Map<ToponymDTO>(entity))
            .Returns(mappedDto);
    }

    private void SetupMocksForFailedQuery()
    {
        _repositoryMock
            .Setup(r => r.ToponymRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Toponym, bool>>>(),  
                It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()))
            .ReturnsAsync((Toponym?)null);
    }

    private void VerifyMocksCalledOnce()
    {
        _repositoryMock.Verify(
            r => r.ToponymRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Toponym, bool>>>(),
                It.IsAny<Func<IQueryable<Toponym>, IIncludableQueryable<Toponym, object>>>()),
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<ToponymDTO>(It.IsAny<Toponym>()),
            Times.Once);
    }

    private void VerifyLoggingOnFailure(GetToponymByIdQuery query)
    {
        _loggerMock.Verify(
            l => l.LogError(query, It.IsAny<string>()),
            Times.Once);
    }
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.MediatR.Toponyms.GetByStreetcodeId;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Toponyms.GetByStreetcodeId;

public class GetToponymsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILoggerService> _loggerMock = new();
    private readonly GetToponymsByStreetcodeIdHandler _handler;

    public GetToponymsByStreetcodeIdHandlerTests()
    {
        _handler = new GetToponymsByStreetcodeIdHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenToponymsExist()
    {
        // Arrange
        var (entities, distinctDtos) = CreateToponymsWithDuplicates();
        SetupRepositoryReturn(entities);
        SetupMapperReturn(distinctDtos);

        var query = new GetToponymsByStreetcodeIdQuery(22);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(distinctDtos);
        VerifyRepositoryCalledOnce();
        _loggerMock.Verify(l => l.LogError(It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnDistinctToponyms_WhenDuplicatesExist()
    {
        // Arrange
        var (entities, distinctDtos) = CreateToponymsWithDuplicates();
        SetupRepositoryReturn(entities);
        SetupMapperReturn(distinctDtos);

        var query  = new GetToponymsByStreetcodeIdQuery(22);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value
              .Select(t => t.StreetName)
              .Should()
              .OnlyHaveUniqueItems()
              .And
              .BeEquivalentTo(new[] { "Main" });
    }

    [Fact]
    public async Task Handle_ShouldCallMapperOncePerDistinctToponym()
    {
        // Arrange
        var (entities, distinctDtos) = CreateToponymsWithDuplicates();
        SetupRepositoryReturn(entities);
        SetupMapperReturn(distinctDtos);

        var query = new GetToponymsByStreetcodeIdQuery(22);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        _ = result.Value.ToList();

        // Assert
        _mapperMock.Verify(
            m => m.Map<ToponymDTO>(It.IsAny<Toponym>()),
            Times.Exactly(distinctDtos.Count()));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResultAndLogError_WhenNoToponymsExist()
    {
        // Arrange
        SetupRepositoryReturn(Array.Empty<Toponym>());

        var query = new GetToponymsByStreetcodeIdQuery(22);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _loggerMock.Verify(
            l => l.LogError(query, $"Cannot find any toponym by the streetcode id: {query.StreetcodeId}"),
            Times.Once);
    }

    private static (IEnumerable<Toponym> entities, IEnumerable<ToponymDTO> dtos) CreateToponymsWithDuplicates()
    {
        var entities = new[]
        {
            new Toponym { Id = 1, StreetName = "Main" },
            new Toponym { Id = 2, StreetName = "Main" }
        };

        var dtos = new[]
        {
            new ToponymDTO { Id = 1, StreetName = "Main" }
        };

        return (entities, dtos);
    }

    private void SetupRepositoryReturn(IEnumerable<Toponym> entities)
    {
        _repositoryMock
            .Setup(r => r.ToponymRepository.GetAllAsync(
                It.IsAny<Expression<Func<Toponym, bool>>>(),
                It.IsAny<Func<IQueryable<Toponym>,
                              IIncludableQueryable<Toponym, object>>>()))
            .ReturnsAsync(entities);
    }

    private void SetupMapperReturn(IEnumerable<ToponymDTO> dtos)
    {
        _mapperMock
            .Setup(m => m.Map<ToponymDTO>(It.IsAny<Toponym>()))
            .Returns<Toponym>(t =>
                dtos.FirstOrDefault(d => d.Id == t.Id) ??
                new ToponymDTO { Id = t.Id, StreetName = t.StreetName });
    }

    private void VerifyRepositoryCalledOnce()
        => _repositoryMock.Verify(
                r => r.ToponymRepository.GetAllAsync(
                    It.IsAny<Expression<Func<Toponym, bool>>>(),
                    It.IsAny<Func<IQueryable<Toponym>,
                                  IIncludableQueryable<Toponym, object>>>()),
                Times.Once);
}

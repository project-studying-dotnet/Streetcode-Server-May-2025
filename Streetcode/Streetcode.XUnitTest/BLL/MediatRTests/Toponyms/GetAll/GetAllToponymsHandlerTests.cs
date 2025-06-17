using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Toponyms.GetAll;
using Streetcode.DAL.Entities.Toponyms;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Toponyms.GetAll;

public class GetAllToponymsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetAllToponymsHandler _handler;

    public GetAllToponymsHandlerTests()
    {
        _handler = new GetAllToponymsHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenToponymsExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateValidToponymEntitiesAndDtos();
        SetupMocksForToponyms(entities, mappedDtos);

        var query = new GetAllToponymsQuery(
            new GetAllToponymsRequestDTO { Title = null, Amount = 10, Page = 1 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllToponyms_WhenToponymsExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateValidToponymEntitiesAndDtos();
        SetupMocksForToponyms(entities, mappedDtos);

        var query = new GetAllToponymsQuery(
            new GetAllToponymsRequestDTO { Title = null, Amount = 10, Page = 1 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Toponyms.Should().BeEquivalentTo(mappedDtos);
        result.Value.Pages.Should().Be(1);
        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenNoToponymsExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateEmptyToponymEntitiesAndDtos();
        SetupMocksForToponyms(entities, mappedDtos);

        var query = new GetAllToponymsQuery(
            new GetAllToponymsRequestDTO { Title = null, Amount = 10, Page = 1 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Toponyms.Should().BeEmpty();
        VerifyMocksCalledOnce();
    }

    private static (IEnumerable<Toponym> entities, IEnumerable<ToponymDTO> dtos)
        CreateValidToponymEntitiesAndDtos()
    {
        var entities = new[]
        {
            new Toponym { Id = 1, StreetName = "First street" },
            new Toponym { Id = 2, StreetName = "Second street" }
        };

        var dtos = new[]
        {
            new ToponymDTO { Id = 1, StreetName = "First street" },
            new ToponymDTO { Id = 2, StreetName = "Second street" }
        };

        return (entities, dtos);
    }

    private static (IEnumerable<Toponym> entities, IEnumerable<ToponymDTO> dtos)
        CreateEmptyToponymEntitiesAndDtos() =>
        (Array.Empty<Toponym>(), Array.Empty<ToponymDTO>());

    private void SetupMocksForToponyms(
        IEnumerable<Toponym> entities,
        IEnumerable<ToponymDTO> mappedDtos)
    {
        _repositoryWrapperMock
            .Setup(r => r.ToponymRepository.FindAll(
                It.IsAny<Expression<Func<Toponym, bool>>>()))
            .Returns(entities.AsQueryable());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ToponymDTO>>(It.IsAny<IEnumerable<Toponym>>()))
            .Returns(mappedDtos);
    }

    private void VerifyMocksCalledOnce()
    {
        _repositoryWrapperMock.Verify(r => r.ToponymRepository.FindAll(
            It.IsAny<Expression<Func<Toponym, bool>>>()), 
            Times.Once);

        _mapperMock.Verify(m =>
            m.Map<IEnumerable<ToponymDTO>>(It.IsAny<IEnumerable<Toponym>>()),
            Times.Once);
    }
}

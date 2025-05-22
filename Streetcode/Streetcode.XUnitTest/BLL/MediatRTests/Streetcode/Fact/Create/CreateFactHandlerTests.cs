using System.Linq.Expressions;
using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Fact.Create;

public class CreateFactHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repoWrapperMock;
    private readonly Mock<IFactRepository> _factRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly CreateFactHandler _handler;

    public CreateFactHandlerTests()
    {
        _repoWrapperMock = new Mock<IRepositoryWrapper>();
        _factRepoMock = new Mock<IFactRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();

        _repoWrapperMock
            .SetupGet(r => r.FactRepository)
            .Returns(_factRepoMock.Object);

        _handler = new CreateFactHandler(
            _repoWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMappingReturnsNull()
    {
        // Arrange
        var dto = new FactUpdateCreateDTO();
        _mapperMock
            .Setup(m => m.Map<Entity>(It.IsAny<FactUpdateCreateDTO>()))
            .Returns((Entity)null);

        // Act
        var result = await _handler.Handle(new CreateFactCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStreetcodeIdIsZero()
    {
        // Arrange
        var dto = new FactUpdateCreateDTO { StreetcodeId = 0, FactContent = "x" };
        var mapped = new Entity { StreetcodeId = 0, FactContent = "x" };
        _mapperMock
            .Setup(m => m.Map<Entity>(It.IsAny<FactDTO>()))
            .Returns(mapped);

        // Act
        var result = await _handler.Handle(new CreateFactCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDuplicateExists()
    {
        // Arrange
        var dto = new FactUpdateCreateDTO { StreetcodeId = 1, FactContent = "dup" };
        var mapped = new Entity { StreetcodeId = 1, FactContent = "dup" };
        _mapperMock
            .Setup(m => m.Map<Entity>(It.IsAny<FactUpdateCreateDTO>()))
            .Returns(mapped);

        _factRepoMock
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Entity, bool>>>(), null))
            .ReturnsAsync(new Entity()); // simulate duplicate

        // Act
        var result = await _handler.Handle(new CreateFactCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSaveChangesSucceeds()
    {
        // Arrange
        var dto = new FactUpdateCreateDTO { StreetcodeId = 1, FactContent = "ok" };
        var mapped = new Entity { StreetcodeId = 1, FactContent = "ok" };
        _mapperMock
            .Setup(m => m.Map<Entity>(It.IsAny<FactUpdateCreateDTO>()))
            .Returns(mapped);

        _factRepoMock
            .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Entity, bool>>>(), null))
            .ReturnsAsync((Entity)null);

        _factRepoMock
            .Setup(r => r.CreateAsync(mapped))
            .ReturnsAsync(mapped);

        _repoWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<FactUpdateCreateDTO>(mapped))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(new CreateFactCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
    {
        // Arrange
        var dto = new FactUpdateCreateDTO { StreetcodeId = 1, FactContent = "fail" };
        var mapped = new Entity { StreetcodeId = 1, FactContent = "fail" };
        _mapperMock
            .Setup(m => m.Map<Entity>(It.IsAny<FactUpdateCreateDTO>()))
            .Returns(mapped);

_factRepoMock
    .Setup(r => r.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Entity, bool>>>(), null))
    .ReturnsAsync(new Entity()); // simulate duplicate

        _factRepoMock
            .Setup(r => r.CreateAsync(mapped))
            .ReturnsAsync(mapped);

        _repoWrapperMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new CreateFactCommand(dto), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }
}
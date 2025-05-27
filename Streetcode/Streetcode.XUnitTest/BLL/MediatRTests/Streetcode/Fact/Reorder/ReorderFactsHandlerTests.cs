using System.Linq.Expressions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Fact.Reorder;

public class ReorderFactsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
    private readonly Mock<ILoggerService> loggerServiceMock;
    private readonly ReorderFactsHandler reorderFactsHandler;

    public ReorderFactsHandlerTests()
    {
        repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        loggerServiceMock = new Mock<ILoggerService>();
        reorderFactsHandler = new ReorderFactsHandler(
            repositoryWrapperMock.Object,
            loggerServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFactReorderDtoListIsEmpty()
    {
        // Arrange
        var emptyFactReorderDtoList = Array.Empty<FactReorderDTO>();
        var reorderFactsCommand = new ReorderFactsCommand(emptyFactReorderDtoList, 123);

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDuplicateFactIdsExist()
    {
        // Arrange
        var duplicateFactReorderDtoList = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 1 },
            new FactReorderDTO { Id = 1, NewPosition = 2 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(duplicateFactReorderDtoList, 123);

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewPositionsAreNotConsecutive()
    {
        // Arrange
        var nonConsecutivePositionsDtoList = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 1 },
            new FactReorderDTO { Id = 2, NewPosition = 3 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(nonConsecutivePositionsDtoList, 123);

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoFactsAreFoundForStreetcodeId()
    {
        // Arrange
        var validFactReorderDtoList = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 1 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(validFactReorderDtoList, 999);

        repositoryWrapperMock
            .Setup(repo => repo.FactRepository.FindAll(
                It.IsAny<Expression<Func<Entity, bool>>>()))
            .Returns(new List<Entity>().AsQueryable());

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFactIdInDtoIsNotFoundInDatabase()
    {
        // Arrange
        var missingFactIdDtoList = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 1 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(missingFactIdDtoList, 5);

        var existingFactsList = new List<Entity>
        {
            new Entity { Id = 2, StreetcodeId = 5, Position = 1 }
        };
        repositoryWrapperMock
            .Setup(repo => repo.FactRepository.FindAll(
                It.IsAny<Expression<Func<Entity, bool>>>()))
            .Returns(existingFactsList.AsQueryable());

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesAffectsNoRows()
    {
        // Arrange
        var singleFactReorderDtoList = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 2 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(singleFactReorderDtoList, 7);

        var factsListFromDatabase = new List<Entity>
        {
            new Entity { Id = 1, StreetcodeId = 7, Position = 1 }
        };
        repositoryWrapperMock
            .Setup(repo => repo.FactRepository.FindAll(
                It.IsAny<Expression<Func<Entity, bool>>>()))
            .Returns(factsListFromDatabase.AsQueryable());

        repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndUpdatePositions_WhenValidRequestIsProvided()
    {
        // Arrange
        var reorderDtos = new List<FactReorderDTO>
        {
            new FactReorderDTO { Id = 1, NewPosition = 2 },
            new FactReorderDTO { Id = 2, NewPosition = 1 }
        };
        var reorderFactsCommand = new ReorderFactsCommand(reorderDtos, 42);

        var factsListFromDatabase = new List<Entity>
        {
            new Entity { Id = 1, StreetcodeId = 42, Position = 1 },
            new Entity { Id = 2, StreetcodeId = 42, Position = 2 }
        };
        repositoryWrapperMock
            .Setup(repo => repo.FactRepository.FindAll(
                It.IsAny<Expression<Func<Entity, bool>>>()))
            .Returns(factsListFromDatabase.AsQueryable());

        repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(2);

        // Act
        var result = await reorderFactsHandler.Handle(
            reorderFactsCommand,
            CancellationToken.None
        );

        // Assert
        Assert.True(result.IsSuccess);
    }
}
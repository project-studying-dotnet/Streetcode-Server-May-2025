using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.Position.GetAll;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.Position.GetAll;

public class GetAllPositionsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllPositionsHandler _handler;

    public GetAllPositionsHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetAllPositionsHandler(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Positions_Exist()
    {
        // Arrange
        var entities = new List<Positions> { new() { Id = 1, Position = "Team Lead" } };
        var dtos = entities.Select(e => new PositionDTO { Id = e.Id, Position = e.Position });

        SetupRepoToReturn(entities);
        _mockMapper.Setup(m => m.Map<IEnumerable<PositionDTO>>(entities)).Returns(dtos);

        // Act
        var result = await _handler.Handle(new GetAllPositionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_Positions_Null()
    {
        // Arrange
        SetupRepoToReturn(null);

        // Act
        var result = await _handler.Handle(new GetAllPositionsQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _mockLogger.Verify(l => l.LogError(It.IsAny<GetAllPositionsQuery>(), It.IsAny<string>()), Times.Once);
    }


    private void SetupRepoToReturn(IEnumerable<Positions>? positions)
    {
        _mockRepo.Setup(r => r.PositionRepository.GetAllAsync(
                It.IsAny<Expression<Func<Positions, bool>>>(),
                It.IsAny<Func<IQueryable<Positions>, IIncludableQueryable<Positions, object>>>()))
            .ReturnsAsync(positions);
    }
}
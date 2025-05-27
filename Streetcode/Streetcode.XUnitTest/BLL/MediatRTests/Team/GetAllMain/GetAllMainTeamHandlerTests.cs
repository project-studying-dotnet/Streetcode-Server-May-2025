using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.GetAll;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.GetAll;

public class GetAllMainTeamHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetAllMainTeamHandler _handler;

    public GetAllMainTeamHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetAllMainTeamHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenDataExists()
    {
        // Arrange
        var (entities, dtos) = CreateTeamEntitiesAndDtos();
        SetupMocksForTeam(entities, dtos);

        // Act
        var result = await _handler.Handle(new GetAllMainTeamQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dtos);
        VerifyMocksCalled(true);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenDataIsEmpty()
    {
        // Arrange
        var entities = new List<TeamMember>();
        var dtos = new List<TeamMemberDTO>();
        SetupMocksForTeam(entities, dtos);

        // Act
        var result = await _handler.Handle(new GetAllMainTeamQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        VerifyMocksCalled(true);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailedResult_WhenDataIsNull()
    {
        // Arrange
        List<TeamMember>? entities = null;
        List<TeamMemberDTO>? dtos = null;
        SetupMocksForTeam(entities, dtos);

        // Act
        var result = await _handler.Handle(new GetAllMainTeamQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _loggerMock.Verify(l => l.LogError(It.IsAny<GetAllMainTeamQuery>(), It.IsAny<string>()), Times.Once);
        VerifyMocksCalled(false);
    }

    private static (List<TeamMember>, List<TeamMemberDTO>) CreateTeamEntitiesAndDtos()
    {
        var entities = new List<TeamMember>
        {
            new TeamMember { Id = 1, IsMain = true },
            new TeamMember { Id = 2, IsMain = true }
        };
        var dtos = new List<TeamMemberDTO>
        {
            new TeamMemberDTO { Id = 1 },
            new TeamMemberDTO { Id = 2 }
        };
        return (entities, dtos);
    }

    private void SetupMocksForTeam(List<TeamMember>? entities, List<TeamMemberDTO>? dtos)
    {
        _repositoryWrapperMock
            .Setup(repo => repo.TeamRepository.GetAllAsync(
                It.IsAny<Expression<Func<TeamMember, bool>>>(),
                It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()))
            .ReturnsAsync(entities);

        if (entities != null)
        {
            _mapperMock
                .Setup(mapper => mapper.Map<IEnumerable<TeamMemberDTO>>(It.IsAny<IEnumerable<TeamMember>>()))
                .Returns(dtos!);
        }
    }

    private void VerifyMocksCalled(bool verifyMapping)
    {
        _repositoryWrapperMock.Verify(repo => repo.TeamRepository.GetAllAsync(
            It.IsAny<Expression<Func<TeamMember, bool>>>(),
            It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()), Times.Once);

        if (verifyMapping)
        {
            _mapperMock.Verify(m => m.Map<IEnumerable<TeamMemberDTO>>(It.IsAny<IEnumerable<TeamMember>>()), Times.Once);
        }
        else
        {
            _mapperMock.Verify(m => m.Map<IEnumerable<TeamMemberDTO>>(It.IsAny<IEnumerable<TeamMember>>()), Times.Never);
        }
    }
}
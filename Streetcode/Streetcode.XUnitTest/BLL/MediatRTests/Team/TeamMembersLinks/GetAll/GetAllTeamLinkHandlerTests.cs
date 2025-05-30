using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.GetAll;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.TeamMembersLinks.GetAll;

public class GetAllTeamLinkHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetAllTeamLinkHandlerTests()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
    }

    private GetAllTeamLinkHandler CreateHandler()
        => new GetAllTeamLinkHandler(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);

    private void SetupRepoToReturn(IEnumerable<TeamMemberLink>? teamLinks)
    {
        _mockRepo.Setup(r => r.TeamLinkRepository.GetAllAsync(
                null,  // predicate
                null)) // include
            .ReturnsAsync(teamLinks);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_TeamLinksExist()
    {
        // Arrange
        var entities = new List<TeamMemberLink>
            {
            new TeamMemberLink { Id = 1, TargetUrl = "https://link1" },
            new TeamMemberLink { Id = 2, TargetUrl = "https://link2" }
            };

        var dtos = new List<TeamMemberLinkDTO>
            {
            new TeamMemberLinkDTO { Id = 1, TargetUrl = "https://link1" },
            new TeamMemberLinkDTO { Id = 2, TargetUrl = "https://link2" }
            };

        SetupRepoToReturn(entities);
        _mockMapper.Setup(m => m.Map<IEnumerable<TeamMemberLinkDTO>>(entities)).Returns(dtos);

        var handler = CreateHandler();
        var query = new GetAllTeamLinkQuery();

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_RepositoryReturnsNull()
    {
        // Arrange
        SetupRepoToReturn(null);

        var handler = CreateHandler();
        var query = new GetAllTeamLinkQuery();

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Cannot find any team links");
        _mockLogger.Verify(l => l.LogError(query, "Cannot find any team links"), Times.Once);
    }
}
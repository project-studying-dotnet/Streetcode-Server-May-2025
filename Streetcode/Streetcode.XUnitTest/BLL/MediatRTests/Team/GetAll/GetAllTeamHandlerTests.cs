using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.MediatR.Team.GetAll;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.GetAll;

public class GetAllTeamHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetAllTeamHandler _handler;

    public GetAllTeamHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetAllTeamHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenTeamMembersExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateValidTeamEntitiesAndMappedDtos();
        SetupMocksForTeam(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTeamQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTeamMembers_WhenTeamMembersExist()
    {
        // Arrange
        var (entities, mappedDtosEnumerable) = CreateValidTeamEntitiesAndMappedDtos();
        var mappedDtos = mappedDtosEnumerable.ToList();

        SetupMocksForTeam(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTeamQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(mappedDtos);
        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenNoTeamMembersExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateEmptyTeamEntitiesAndMappedDtos();
        SetupMocksForTeam(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTeamQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        VerifyMocksCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTeamMembersExist()
    {
        // Arrange
        var (entities, mappedDtos) = CreateEmptyTeamEntitiesAndMappedDtos();
        SetupMocksForTeam(entities, mappedDtos);

        // Act
        var result = await _handler.Handle(new GetAllTeamQuery(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
        VerifyMocksCalledOnce();
    }

    private static (IEnumerable<TeamMember>, IEnumerable<TeamMemberDTO>) CreateValidTeamEntitiesAndMappedDtos()
    {
        var entities = new List<TeamMember>
        {
            new() { Id = 1, FirstName = "Andrii", LastName = "Osetskyi" },
        };
        var mappedDtos = new List<TeamMemberDTO>
        {
            new() { Id = 1, FirstName = "Andrii", LastName = "Osetskyi" },
        };

        return (entities, mappedDtos);
    }

    private static (IEnumerable<TeamMember>, IEnumerable<TeamMemberDTO>) CreateEmptyTeamEntitiesAndMappedDtos()
    {
        return (new List<TeamMember>(), new List<TeamMemberDTO>());
    }

    private void SetupMocksForTeam(IEnumerable<TeamMember> entities, IEnumerable<TeamMemberDTO> mappedDtos)
    {
        _repositoryWrapperMock
            .Setup(repo => repo.TeamRepository.GetAllAsync(
                It.IsAny<Expression<Func<TeamMember, bool>>>(),
                It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()))
            .ReturnsAsync(entities);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<TeamMemberDTO>>(entities))
            .Returns(mappedDtos);
    }

    private void VerifyMocksCalledOnce()
    {
        _repositoryWrapperMock.Verify(repo => repo.TeamRepository.GetAllAsync(
            It.IsAny<Expression<Func<TeamMember, bool>>>(),
            It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()),
            Times.Once);

        _mapperMock.Verify(mapper =>
            mapper.Map<IEnumerable<TeamMemberDTO>>(It.IsAny<IEnumerable<TeamMember>>()),
            Times.Once);
    }
}

using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.GetById;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.GetById
{
    public class GetByIdTeamHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetByIdTeamHandler _handler;

        public GetByIdTeamHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetByIdTeamHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnOkResult_WhenTeamExists()
        {
            var (entity, mappedDto, targetId) = CreateValidTeamEntityAndDto();
            SetupMocksForTeam(entity, mappedDto);

            var result = await _handler.Handle(new GetByIdTeamQuery(targetId), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            VerifyMocksCalledOnce();
        }

        [Fact]
        public async Task Handle_ShouldReturnTeamMemberDto_WhenTeamExists()
        {
            var (entity, mappedDto, targetId) = CreateValidTeamEntityAndDto();
            SetupMocksForTeam(entity, mappedDto);

            var result = await _handler.Handle(new GetByIdTeamQuery(targetId), CancellationToken.None);

            result.Value.Should().BeEquivalentTo(mappedDto);
            VerifyMocksCalledOnce();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenTeamDoesNotExist()
        {
            var (entity, mappedDto, targetId) = CreateNullTeamEntityAndDto();
            SetupMocksForTeam(entity, mappedDto);

            var result = await _handler.Handle(new GetByIdTeamQuery(targetId), CancellationToken.None);

            result.IsFailed.Should().BeTrue();
            VerifyMocksCalledOnce(verifyMapping: false);
        }

        private static (TeamMember?, TeamMemberDTO?, int) CreateNullTeamEntityAndDto()
        {
            const int targetId = 1;
            TeamMember? entity = null;
            TeamMemberDTO? dto = null;
            return (entity, dto, targetId);
        }

        private static (TeamMember, TeamMemberDTO, int) CreateValidTeamEntityAndDto()
        {
            const int targetId = 1;
            var entity = new TeamMember { Id = targetId, FirstName = "Andrii", LastName = "Osetskyi" };
            var dto = new TeamMemberDTO { Id = targetId, FirstName = "Andrii", LastName = "Osetskyi" };
            return (entity, dto, targetId);
        }

        private void SetupMocksForTeam(TeamMember? entity, TeamMemberDTO? dto)
        {
            _repositoryWrapperMock
                .Setup(repo => repo.TeamRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<TeamMember, bool>>>(),
                    It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(mapper => mapper.Map<TeamMemberDTO>(It.IsAny<TeamMember>()))
                .Returns(dto!);
        }

        private void VerifyMocksCalledOnce(bool verifyMapping = true)
        {
            _repositoryWrapperMock.Verify(repo => repo.TeamRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<TeamMember, bool>>>(),
                It.IsAny<Func<IQueryable<TeamMember>, IIncludableQueryable<TeamMember, object>>>()), Times.Once);

            if (verifyMapping)
            {
                _mapperMock.Verify(mapper => mapper.Map<TeamMemberDTO>(It.IsAny<TeamMember>()), Times.Once);
            }
            else
            {
                _mapperMock.Verify(mapper => mapper.Map<TeamMemberDTO>(It.IsAny<TeamMember>()), Times.Never);
            }
        }
    }
}

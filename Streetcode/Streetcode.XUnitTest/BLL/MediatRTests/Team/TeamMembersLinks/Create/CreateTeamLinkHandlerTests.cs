using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.Position.GetAll;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;


namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.TeamMembersLinks.Create
{
    public class CreateTeamLinkHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;

        public CreateTeamLinkHandlerTests()
        {
            _mockRepo = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
        }

        private CreateTeamLinkHandler CreateHandler() =>
            new(_mockMapper.Object, _mockRepo.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_Should_Return_Fail_If_MappingRequestIsNull()
        {
            // Arrange
            var query = new CreateTeamLinkQuery(null);
            _mockMapper.Setup(m => m.Map<DAL.Entities.Team.TeamMemberLink>(null))
                .Returns((DAL.Entities.Team.TeamMemberLink)null);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Cannot convert null to team link");
            _mockLogger.Verify(l => l.LogError(query, "Cannot convert null to team link"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_If_CreateAsync_ReturnsNull()
        {
            // Arrange
            var dto = new TeamMemberLinkDTO();
            var entity = new DAL.Entities.Team.TeamMemberLink();

            var query = new CreateTeamLinkQuery(dto);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Team.TeamMemberLink>(dto)).Returns(entity);
            _mockRepo.Setup(r => r.TeamLinkRepository.CreateAsync(entity))
                .ReturnsAsync((DAL.Entities.Team.TeamMemberLink)null);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Cannot create team link");
            _mockLogger.Verify(l => l.LogError(query, "Cannot create team link"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_If_SaveChangesFails()
        {
            // Arrange
            var dto = new TeamMemberLinkDTO();
            var entity = new DAL.Entities.Team.TeamMemberLink();
            var createdEntity = new DAL.Entities.Team.TeamMemberLink();

            var query = new CreateTeamLinkQuery(dto);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Team.TeamMemberLink>(dto)).Returns(entity);
            _mockRepo.Setup(r => r.TeamLinkRepository.CreateAsync(entity))
                .ReturnsAsync(createdEntity);

            _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Failed to create a team");
            _mockLogger.Verify(l => l.LogError(query, "Failed to create a team"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_If_MappingCreatedEntityFails()
        {
            // Arrange
            var dto = new TeamMemberLinkDTO();
            var entity = new DAL.Entities.Team.TeamMemberLink();
            var createdEntity = new DAL.Entities.Team.TeamMemberLink();

            var query = new CreateTeamLinkQuery(dto);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Team.TeamMemberLink>(dto)).Returns(entity);
            _mockRepo.Setup(r => r.TeamLinkRepository.CreateAsync(entity))
                .ReturnsAsync(createdEntity);
            _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<TeamMemberLinkDTO>(createdEntity)).Returns((TeamMemberLinkDTO)null);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Be("Failed to map created team link");
            _mockLogger.Verify(l => l.LogError(query, "Failed to map created team link"), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Success_When_AllOk()
        {
            // Arrange
            var dto = new TeamMemberLinkDTO();
            var entity = new DAL.Entities.Team.TeamMemberLink();
            var createdEntity = new DAL.Entities.Team.TeamMemberLink();
            var mappedDto = new TeamMemberLinkDTO();

            var query = new CreateTeamLinkQuery(dto);

            _mockMapper.Setup(m => m.Map<DAL.Entities.Team.TeamMemberLink>(dto)).Returns(entity);
            _mockRepo.Setup(r => r.TeamLinkRepository.CreateAsync(entity))
                .ReturnsAsync(createdEntity);
            _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<TeamMemberLinkDTO>(createdEntity)).Returns(mappedDto);

            var handler = CreateHandler();

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(mappedDto);
        }
    }
}

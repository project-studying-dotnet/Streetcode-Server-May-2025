using System;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Team.Create;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Team.Position.Create
{
    public class CreatePositionHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CreatePositionHandler _handler;

        public CreatePositionHandlerTests()
        {
            _mockRepo = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new CreatePositionHandler(_mockMapper.Object, _mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_Should_Return_Success_When_Position_Created()
        {
            // Arrange
            var query = CreateTestQuery();
            var entity = CreateTestEntity();
            var dto = CreateTestDto();

            _mockRepo.Setup(r => r.PositionRepository.CreateAsync(It.IsAny<Positions>()))
                     .ReturnsAsync(entity);

            _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<PositionDTO>(entity)).Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Handle_Should_Return_Fail_When_SaveChanges_Throws()
        {
            // Arrange
            var query = CreateTestQuery();
            var entity = CreateTestEntity();
            var exceptionMessage = "DB Error";

            _mockRepo.Setup(r => r.PositionRepository.CreateAsync(It.IsAny<Positions>()))
                     .ReturnsAsync(entity);

            _mockRepo.Setup(r => r.SaveChangesAsync())
                     .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(exceptionMessage);

            _mockLogger.Verify(l => l.LogError(query, exceptionMessage), Times.Once);
        }

        private static CreatePositionQuery CreateTestQuery()
        {
            return new CreatePositionQuery(new PositionDTO { Position = "Test Position" });
        }

        private static Positions CreateTestEntity()
        {
            return new Positions { Id = 1, Position = "Test Position" };
        }

        private static PositionDTO CreateTestDto()
        {
            return new PositionDTO { Id = 1, Position = "Test Position" };
        }
    }
}

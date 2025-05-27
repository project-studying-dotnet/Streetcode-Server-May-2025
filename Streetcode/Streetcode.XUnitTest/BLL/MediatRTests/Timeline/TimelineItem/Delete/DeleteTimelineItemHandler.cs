using System;
using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeleteTimelineItemHandler _handler;

        public DeleteTimelineItemHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new DeleteTimelineItemHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenEntityExistsAndDeleted()
        {
            // Arrange
            var entity = CreateEntity(1);
            var dto = CreateDto(entity.Id);
            SetupMocks(entity, 1, dto);

            var command = new DeleteTimelineItemCommand(entity.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(dto);
            VerifyMocksCalled(entity);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenEntityDoesNotExist()
        {
            // Arrange
            _repositoryWrapperMock
                .Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    null,
                    false))
                .ReturnsAsync((TimelineItemEntity?)null);

            var command = new DeleteTimelineItemCommand(123);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenSaveFails()
        {
            // Arrange
            var entity = CreateEntity(2);
            SetupMocks(entity, 0, null);

            var command = new DeleteTimelineItemCommand(entity.Id);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain("Failed to delete");
            VerifyMocksCalled(entity, verifyMapping: false);
        }
        private static TimelineItemEntity CreateEntity(int id) =>
            new()
            {
                Id = id,
                Title = $"Item {id}",
                Description = "Sample",
                Date = DateTime.Now
            };

        private static TimelineItemDTO CreateDto(int id) =>
            new()
            {
                Id = id,
                Title = $"Item {id}",
                Description = "Sample",
                Date = DateTime.Now
            };

        private void SetupMocks(TimelineItemEntity entity, int saveResult, TimelineItemDTO? dto)
        {
            _repositoryWrapperMock
                .Setup(r => r.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    null,
                    false))
                .ReturnsAsync(entity);

            _repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(saveResult);

            if (dto != null)
            {
                _mapperMock
                    .Setup(m => m.Map<TimelineItemDTO>(entity))
                    .Returns(dto);
            }
        }

        private void VerifyMocksCalled(TimelineItemEntity entity, bool verifyMapping = true)
        {
            _repositoryWrapperMock.Verify(r =>
                r.TimelineRepository.Delete(entity), Times.Once);

            _repositoryWrapperMock.Verify(r =>
                r.SaveChangesAsync(), Times.Once);

            if (verifyMapping)
            {
                _mapperMock.Verify(m =>
                    m.Map<TimelineItemDTO>(entity), Times.Once);
            }
        }
    }
}

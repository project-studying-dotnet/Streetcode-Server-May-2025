using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.GetAll
{
    public class GetAllTimelineItemsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetAllTimelineItemsHandler _handler;

        public GetAllTimelineItemsHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetAllTimelineItemsHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTimelineItems_WhenItemsExist()
        {
            var timelineItems = new List<TimelineItemEntity>
        {
            new TimelineItemEntity
            {
                Id = 1,
                Date = new DateTime(2023, 1, 1),
                Title = "Title 1",
                Description = "Description 1",
            },
        };

            var timelineItemDTOs = new List<TimelineItemDTO>
        {
            new TimelineItemDTO
            {
                Id = 1,
                Date = new DateTime(2023, 1, 1),
                Title = "Title 1",
                Description = "Description 1",
            },
        };

            _mockRepositoryWrapper
                .Setup(repo => repo.TimelineRepository.GetAllAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(timelineItems);

            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<TimelineItemDTO>>(timelineItems))
                .Returns(timelineItemDTOs);

            var result = await _handler.Handle(new GetAllTimelineItemsQuery(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal(timelineItemDTOs, result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenItemsDoNotExist()
        {
            _mockRepositoryWrapper
                .Setup(repo => repo.TimelineRepository.GetAllAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()
                ))
                .ReturnsAsync((IEnumerable<TimelineItemEntity>)null);

            var result = await _handler.Handle(new GetAllTimelineItemsQuery(), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot find any timelineItem", result.Errors[0].Message);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<object>(), "Cannot find any timelineItem"), Times.Once);
        }
    }
}

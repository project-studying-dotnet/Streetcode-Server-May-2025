using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;


namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.GetById
{
    public class GetTimelineItemByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetTimelineItemByIdHandler _handler;

        public GetTimelineItemByIdHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetTimelineItemByIdHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTimelineItem_WhenItemExists()
        {
            var testTimelineItem = new TimelineItemEntity
            {
                Id = 1,
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.Now,
                HistoricalContextTimelines = new List<HistoricalContextTimeline>
                {
                    new HistoricalContextTimeline
                    {
                        HistoricalContext = new HistoricalContext { Id = 1, Title = "Historical Context 1" }
                    }
                }
            };

            var expectedDto = new TimelineItemDTO
            {
                Id = 1,
                Title = "Test Title",
                Description = "Test Description",
                HistoricalContexts = new List<HistoricalContextDTO>
                {
                    new HistoricalContextDTO { Id = 1, Title = "Historical Context 1" }
                }
            };

            _mockRepositoryWrapper
                .Setup(repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()
                ))
                .ReturnsAsync(testTimelineItem);

            _mockMapper.Setup(mapper => mapper.Map<TimelineItemDTO>(testTimelineItem))
                .Returns(expectedDto);

            var query = new GetTimelineItemByIdQuery(1);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedDto.Id, result.Value.Id);
            Assert.Equal(expectedDto.Title, result.Value.Title);
            Assert.Single(result.Value.HistoricalContexts);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenItemNotFound()
        {
            _mockRepositoryWrapper
                .Setup(repo => repo.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()
                ))
                .ReturnsAsync((TimelineItemEntity)null);

            var query = new GetTimelineItemByIdQuery(1);
            var expectedErrorMessage = $"Cannot find a timeline item with corresponding id: {query.Id}";

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailed);
            Assert.Equal(expectedErrorMessage, result.Errors.First().Message);

            _mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<GetTimelineItemByIdQuery>(q => q.Id == query.Id),
                    expectedErrorMessage),
                Times.Once);
        }
    }
}


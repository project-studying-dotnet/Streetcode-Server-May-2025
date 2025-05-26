using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.GetByStreetcodeId;

public class GetTimelineItemsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetTimelineItemsByStreetcodeIdHandler _handler;

    public GetTimelineItemsByStreetcodeIdHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetTimelineItemsByStreetcodeIdHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTimelineItems_WhenItemsExistForStreetcode()
    {
        const int testStreetcodeId = 1;
        var testTimelineItems = new List<TimelineItemEntity>
            {
                new TimelineItemEntity
                {
                    Id = 1,
                    StreetcodeId = testStreetcodeId,
                    Title = "Event 1",
                    HistoricalContextTimelines = new List<HistoricalContextTimeline>
                    {
                        new HistoricalContextTimeline { HistoricalContext = new HistoricalContext { Id = 1 } }
                    }
                },
                new TimelineItemEntity
                {
                    Id = 2,
                    StreetcodeId = testStreetcodeId,
                    Title = "Event 2",
                    HistoricalContextTimelines = new List<HistoricalContextTimeline>()
                }
            };

        var expectedDtos = new List<TimelineItemDTO>
            {
                new TimelineItemDTO { Id = 1, Title = "Event 1" },
                new TimelineItemDTO { Id = 2, Title = "Event 2" }
            };

        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetAllAsync(
                It.Is<Expression<Func<TimelineItemEntity, bool>>>(pred =>
                    PredicateMatchesStreetcodeId(pred, testStreetcodeId)),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()
            ))
            .ReturnsAsync(testTimelineItems);

        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<TimelineItemDTO>>(testTimelineItems))
            .Returns(expectedDtos);

        var query = new GetTimelineItemsByStreetcodeIdQuery(testStreetcodeId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(expectedDtos.First().Title, result.Value.First().Title);
        _mockRepositoryWrapper.VerifyAll();
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenNoItemsFoundForStreetcode()
    {
        const int testStreetcodeId = 999;
        var expectedErrorMsg = $"Cannot find any timeline item by the streetcode id: {testStreetcodeId}";

        _mockRepositoryWrapper
            .Setup(repo => repo.TimelineRepository.GetAllAsync(
                It.Is<Expression<Func<TimelineItemEntity, bool>>>(pred =>
                    PredicateMatchesStreetcodeId(pred, testStreetcodeId)),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()
            ))
            .ReturnsAsync((IEnumerable<TimelineItemEntity>)null);

        var query = new GetTimelineItemsByStreetcodeIdQuery(testStreetcodeId);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
        _mockLogger.Verify(
            x => x.LogError(query, expectedErrorMsg),
            Times.Once);
    }

    private static bool PredicateMatchesStreetcodeId(
        Expression<Func<TimelineItemEntity, bool>> predicate,
        int expectedStreetcodeId)
    {
        var compiled = predicate.Compile();
        var testEntity = new TimelineItemEntity { StreetcodeId = expectedStreetcodeId };
        return compiled(testEntity);
    }
}
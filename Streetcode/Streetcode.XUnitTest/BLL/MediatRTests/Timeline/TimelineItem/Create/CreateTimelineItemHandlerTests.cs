using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.Create;

public class CreateTimelineItemHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateTimelineItemHandler _handler;

    public CreateTimelineItemHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new CreateTimelineItemHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenTimelineItemIsCreatedSuccessfully()
    {
        // Arrange
        var dto = new TimelineItemDTO
        {
            Id = 0,
            Title = "Test Title",
            Date = DateTime.UtcNow,
            DateViewPattern = DateViewPattern.DateMonthYear,
            HistoricalContexts = new List<HistoricalContextDTO> { new() { Id = 1, Title = "War" } }
        };

        var request = new CreateTimelineItemQuery(dto);

        var entity = new TimelineItemEntity
        {
            StreetcodeId = 1,
            HistoricalContextTimelines = new List<HistoricalContextTimeline>()
        };

        _mockMapper.Setup(m => m.Map<TimelineItemEntity>(dto)).Returns(entity);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync(new StreetcodeContent { Id = 1 });
        _mockRepositoryWrapper.Setup(r => r.TimelineRepository.Create(entity));
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<TimelineItemDTO>(entity)).Returns(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenStreetcodeIdIsZero()
    {
        // Arrange
        var dto = new TimelineItemDTO
        {
            Title = "Invalid",
            Date = DateTime.UtcNow,
            DateViewPattern = DateViewPattern.DateMonthYear
        };

        var entity = new TimelineItemEntity { StreetcodeId = 0 };

        _mockMapper.Setup(m => m.Map<TimelineItemEntity>(dto)).Returns(entity);

        var query = new CreateTimelineItemQuery(dto);
        var errorMsg = "StreetcodeId must be a positive number";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.Errors.First().Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenStreetcodeNotFound()
    {
        // Arrange
        var dto = new TimelineItemDTO { Title = "Test", Date = DateTime.UtcNow, DateViewPattern = DateViewPattern.DateMonthYear };
        var entity = new TimelineItemEntity { StreetcodeId = 99 };

        _mockMapper.Setup(m => m.Map<TimelineItemEntity>(dto)).Returns(entity);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync((StreetcodeContent?)null);

        var query = new CreateTimelineItemQuery(dto);
        var errorMsg = $"Streetcode with id {entity.StreetcodeId} does not exist";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.Errors.First().Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSaveFails()
    {
        var dto = new TimelineItemDTO { Title = "Test", Date = DateTime.UtcNow, DateViewPattern = DateViewPattern.DateMonthYear };
        var entity = new TimelineItemEntity { StreetcodeId = 1 };

        var mockTimelineRepo = new Mock<ITimelineRepository>();
        _mockRepositoryWrapper.Setup(r => r.TimelineRepository).Returns(mockTimelineRepo.Object);
        mockTimelineRepo.Setup(r => r.Create(It.IsAny<TimelineItemEntity>()));

        _mockMapper.Setup(m => m.Map<TimelineItemEntity>(dto)).Returns(entity);
        _mockRepositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync(new StreetcodeContent { Id = 1 });
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var query = new CreateTimelineItemQuery(dto);
        var errorMsg = $"Failed to save timeline item.";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMsg, result.Errors.First().Message);
    }
}
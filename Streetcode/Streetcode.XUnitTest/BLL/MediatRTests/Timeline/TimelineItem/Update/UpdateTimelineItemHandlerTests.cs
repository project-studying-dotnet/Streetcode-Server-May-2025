using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Timeline.TimelineItem.Update;

public class UpdateTimelineItemHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly UpdateTimelineItemHandler _handler;

    public UpdateTimelineItemHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new UpdateTimelineItemHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTimelineItemNotFound_ShouldReturnFailResult()
    {
        // Arrange
        var command = new UpdateTimelineItemCommand(1, new TimelineItemDTO());

        _repositoryWrapperMock.Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Timeline.TimelineItem, bool>>>(),
                null))
            .ReturnsAsync((TimelineItemEntity)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Cannot find TimelineItem");
        _loggerMock.Verify(x => x.LogError(command, "Cannot find TimelineItem"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldReturnFailResult()
    {
        // Arrange
        var command = new UpdateTimelineItemCommand(1, new TimelineItemDTO { Id = 1, Title = "Updated Title" });
        var existingItem = new TimelineItemEntity { Id = 1, Title = "Original Title" };

        _repositoryWrapperMock.Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(), null))
            .ReturnsAsync(existingItem);

        _mapperMock.Setup(x => x.Map(command.TimelineItem, existingItem))
            .Callback<TimelineItemDTO, TimelineItemEntity>((dto, entity) =>
            {
                entity.Title = dto.Title;
            });

        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Failed to update TimelineItem");
        _loggerMock.Verify(x => x.LogError(command, "Failed to update TimelineItem"), Times.Once);
        _repositoryWrapperMock.Verify(x => x.TimelineRepository.Update(existingItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesSucceeds_ShouldReturnSuccessResult()
    {
        // Arrange
        var timelineItemDto = new TimelineItemDTO
        {
            Id = 1,
            Title = "Updated Title",
            Date = DateTime.Now,
            DateViewPattern = DateViewPattern.DateMonthYear,
            Description = "Updated Description",
            HistoricalContexts = new List<HistoricalContextDTO>()
        };

        var command = new UpdateTimelineItemCommand(1, timelineItemDto);
        var existingItem = new TimelineItemEntity
        {
            Id = 1,
            Title = "Original Title",
            Date = DateTime.Now,
            DateViewPattern = DateViewPattern.DateMonthYear,
            Description = "Original Description"
        };

        _repositoryWrapperMock.Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(), null))
            .ReturnsAsync(existingItem);

        _mapperMock.Setup(x => x.Map(command.TimelineItem, existingItem))
            .Callback<TimelineItemDTO, TimelineItemEntity>((dto, entity) =>
            {
                entity.Id = dto.Id;
                entity.Title = dto.Title;
                entity.Date = dto.Date;
                entity.Description = dto.Description;
            });

        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(timelineItemDto, options => options
            .IncludingAllDeclaredProperties());
        _repositoryWrapperMock.Verify(x => x.TimelineRepository.Update(existingItem), Times.Once);
    }
}
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
public record UpdateTimelineItemCommand(int Id, TimelineItemDTO TimelineItem) : IRequest<Result<TimelineItemDTO>>;
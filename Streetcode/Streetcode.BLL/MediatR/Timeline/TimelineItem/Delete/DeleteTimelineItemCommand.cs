using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;

public record DeleteTimelineItemCommand(int Id) : IRequest<Result<TimelineItemDTO>>;

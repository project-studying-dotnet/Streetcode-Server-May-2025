using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;

namespace Streetcode.WebApi.Controllers.Timeline;

public class TimelineItemController : BaseApiController
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteTimelineItemCommand(id)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TimelineItemDTO newTimelineItem)
    {
        return HandleResult(await Mediator.Send(new CreateTimelineItemQuery(newTimelineItem)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllTimelineItemsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetTimelineItemByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetTimelineItemsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPut("{streetcodeId:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TimelineItemDTO timelineItem)
    {
        return HandleResult(await Mediator.Send(new UpdateTimelineItemCommand(id, timelineItem)));
    }
}

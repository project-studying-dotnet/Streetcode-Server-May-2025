using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.MediatR.Analytics;
using Streetcode.BLL.MediatR.Analytics.Create;
using Streetcode.BLL.MediatR.Analytics.Delete;

namespace Streetcode.WebApi.Controllers.Analytics;

public class StatisticRecordController : BaseApiController
{
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StatisticRecordCreateDTO statisticRecordCreateDTO)
    {
        return HandleResult(await Mediator.Send(new CreateStatisticRecordCommand(statisticRecordCreateDTO)));
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteStatisticRecordCommand(id)));
    }
}
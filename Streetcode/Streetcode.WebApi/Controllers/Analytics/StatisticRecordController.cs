using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.MediatR.Analytics;

namespace Streetcode.WebApi.Controllers.Analytics;

public class StatisticRecordController : BaseApiController
{
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StatisticRecordCreateDTO statisticRecordCreateDTO)
    {
        return HandleResult(await Mediator.Send(new CreateStatisticRecordCommand(statisticRecordCreateDTO)));
    }
}

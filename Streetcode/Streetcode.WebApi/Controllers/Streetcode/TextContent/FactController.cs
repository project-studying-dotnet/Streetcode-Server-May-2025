using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class FactController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllFactsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetFactByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetFactByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FactUpdateCreateDTO fact)
    {
        return HandleResult(await Mediator.Send(new CreateFactCommand(fact)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] FactDTO fact)
    {
        return HandleResult(await Mediator.Send(new UpdateFactsCommand(fact)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteFactCommand(id)));
    }

    [HttpPatch("{streetcodeId:int}/Reorder")]
    public async Task<IActionResult> Reorder([FromBody] IEnumerable<FactReorderDTO> factReorderDtos, [FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new ReorderFactsCommand(factReorderDtos, streetcodeId)));
    }
}

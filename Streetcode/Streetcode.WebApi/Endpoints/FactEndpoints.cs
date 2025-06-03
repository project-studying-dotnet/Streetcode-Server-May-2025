using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;

namespace Streetcode.WebApi.Endpoints;

public static class FactEndpoints
{
    public static void MapFactEndpoints(this IEndpointRouteBuilder app)
    {

       
    }

    [HttpGet]
    private static async Task<IResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllFactsQuery()));
    }

    [HttpGet("{id:int}")]
    private static async Task<IResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetFactByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    private static async Task<IResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetFactByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    private static async Task<IResult> Create([FromBody] FactUpdateCreateDTO fact)
    {
        return HandleResult(await Mediator.Send(new CreateFactCommand(fact)));
    }

    [HttpPut]
    private static async Task<IResult> Update([FromBody] FactDTO fact)
    {
        return HandleResult(await Mediator.Send(new UpdateFactsCommand(fact)));
    }

    [HttpDelete("{id:int}")]
    private static async Task<IResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteFactCommand(id)));
    }

    [HttpPatch("{streetcodeId:int}/Reorder")]
    private static async Task<IResult> Reorder([FromBody] IEnumerable<FactReorderDTO> factReorderDtos, [FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new ReorderFactsCommand(factReorderDtos, streetcodeId)));
    }
}
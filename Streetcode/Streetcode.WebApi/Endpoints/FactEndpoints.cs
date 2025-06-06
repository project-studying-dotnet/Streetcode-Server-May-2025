using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Forms;
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
        var group = app.MapGroup("api/Fact")
            .WithTags("FactMinimalApi")
            .WithOpenApi();

        group.MapGet("api/Fact/GetAll", GetAll);
        group.MapGet("GetById/{id:int}", GetById);
        group.MapGet("ByStreetcode/{streetcodeId:int}", GetByStreetcodeId);
        group.MapPost("Create", Create);
        group.MapPut("Update", Update);
        group.MapPatch("{streetcodeId:int}/Reorder", Reorder);
        group.MapDelete("Delete/{id:int}", Delete);
    }

    private static async Task<IResult> GetAll(IMediator mediator)
    {
        var logicResult = await mediator.Send(new GetAllFactsQuery());

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> GetById(int id, IMediator mediator)
    {
        var logicResult = await mediator.Send(new GetFactByIdQuery(id));

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> GetByStreetcodeId(int streetcodeId, IMediator mediator)
    {
        var logicResult = await mediator.Send(new GetFactByStreetcodeIdQuery(streetcodeId));

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> Create(FactUpdateCreateDTO fact, IMediator mediator)
    {
        var logicResult = await mediator.Send(new CreateFactCommand(fact));

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> Update(FactDTO fact, IMediator mediator)
    {
        var logicResult = await mediator.Send(new UpdateFactsCommand(fact));

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> Delete(int id, IMediator mediator)
    {
        var logicResult = await mediator.Send(new DeleteFactCommand(id));

        return ApiResultMapper.HandleResult(logicResult);
    }

    private static async Task<IResult> Reorder(
        int streetcodeId,
        [FromBody] IEnumerable<FactReorderDTO> factReorderDtos,
        IMediator mediator)
    {
        var logicResult = await mediator.Send(new ReorderFactsCommand(factReorderDtos, streetcodeId));

        return ApiResultMapper.HandleResult(logicResult);
    }
}
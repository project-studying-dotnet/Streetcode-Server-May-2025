using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetPending;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;

namespace Streetcode.WebApi.Controllers.Streetcode;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        var result = await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId));
        return HandleResult(result);
    }

    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int commentId)
    {
        var result = await Mediator.Send(new DeleteCommentCommand(commentId));

        return HandleResult(result);
    }

    [HttpGet("admin/pending")]
    public async Task<IActionResult> GetPendingComments()
    {
        var result = await Mediator.Send(new GetPendingCommentsQuery());

        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCommentDTO updateCommentDto)
    {
        var result = await Mediator.Send(new UpdateCommentCommand(updateCommentDto));

        return HandleResult(result);
    }
} 

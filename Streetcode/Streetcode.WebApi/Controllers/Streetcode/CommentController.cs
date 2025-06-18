using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetPending;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetById;
using Streetcode.DAL.Enums;

namespace Streetcode.WebApi.Controllers.Streetcode;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpGet("pending")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> GetPending()
    {
        return HandleResult(await Mediator.Send(new GetPendingCommentsQuery()));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetCommentByIdQuery(id)));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCommentDTO comment)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(comment)));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentDTO comment)
    {
        comment.Id = id;
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(comment)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteCommentCommand(id)));
    }
}

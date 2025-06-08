using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

namespace Streetcode.WebApi.Controllers.Streetcode;

public class CommentController : BaseApiController
{
    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        var result = await Mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId));
        return HandleResult(result);
    }
} 
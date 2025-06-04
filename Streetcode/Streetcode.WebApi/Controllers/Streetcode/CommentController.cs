using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

namespace Streetcode.WebApi.Controllers.Streetcode;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-streetcode/{streetcodeId}")]
    public async Task<IActionResult> GetByStreetcodeId(int streetcodeId)
    {
        var result = await _mediator.Send(new GetCommentsByStreetcodeIdQuery(streetcodeId));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Errors);
    }
} 
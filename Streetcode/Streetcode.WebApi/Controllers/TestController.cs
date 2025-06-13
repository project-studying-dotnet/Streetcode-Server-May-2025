using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Streetcode.WebApi.Controllers;
public class TestController : BaseApiController
{
    private readonly IPublishEndpoint _publishEndpoint;

    public TestController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        await _publishEndpoint.Publish(
        new TestConnectionEvent
        {
            Message = "test"
        });
        return Ok();
    }
}

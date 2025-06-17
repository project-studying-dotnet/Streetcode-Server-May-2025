using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.MainPage.Create;
using Streetcode.BLL.MediatR.Streetcode.MainPage.Delete;

namespace Streetcode.WebApi.Controllers.Streetcode;

[ApiController]
[Route("api/[controller]")]
public class MainPageController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StreetcodeMainPageCreateDTO dto)
    {
        var result = await Mediator.Send(new CreateMainStreetcodeCommand(dto));
        return HandleResult(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] StreetcodeMainPageDeleteDTO dto)
    {
        var result = await Mediator.Send(new DeleteMainStreetcodeCommand(dto));
        return HandleResult(result);
    }
}

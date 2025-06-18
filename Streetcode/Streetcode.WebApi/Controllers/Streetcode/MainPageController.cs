using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.MainPage.Create;

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
}

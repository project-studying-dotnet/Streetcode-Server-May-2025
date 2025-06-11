using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.BLL.MediatR.Partners.GetAllPartnerShort;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.DAL.Enums;

namespace Streetcode.WebApi.Controllers.Partners;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = HandleResult(await Mediator.Send(new GetAllPartnersQuery()));
        return result;
    }

    [HttpGet("short")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllShort()
    {
        return HandleResult(await Mediator.Send(new GetAllPartnersShortQuery()));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetPartnerByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetPartnersByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> Create([FromBody] CreatePartnerDTO partner)
    {
        return HandleResult(await Mediator.Send(new CreatePartnerCommand(partner)));
    }

    [HttpPut]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> Update([FromBody] CreatePartnerDTO partner)
    {
        return HandleResult(await Mediator.Send(new BLL.MediatR.Partners.Update.UpdatePartnerCommand(partner)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.Administrator)},{nameof(UserRole.MainAdministrator)}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new BLL.MediatR.Partners.Delete.DeletePartnerCommand(id)));
    }
}

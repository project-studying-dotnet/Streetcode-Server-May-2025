using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.BLL.MediatR.News.Delete;
using Streetcode.BLL.MediatR.News.GetAll;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Streetcode.BLL.MediatR.News.SortedByDateTime;
using Streetcode.BLL.MediatR.News.Update;

namespace Streetcode.WebApi.Controllers.News;

public class NewsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllNewsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetNewsByIdQuery(id)));
    }
    
    [HttpGet("{url}")]
    public async Task<IActionResult> GetByUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new GetNewsByUrlQuery(url)));
    }
    
    [HttpGet("{url}")]
    public async Task<IActionResult> GetNewsAndLinksByUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new GetNewsAndLinksByUrlQuery(url)));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetSortedByDateTime()
    {
        return HandleResult(await Mediator.Send(new SortedByDateTimeQuery()));
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NewsDTO newsDto)
    {
        return HandleResult(await Mediator.Send(new CreateNewsCommand(newsDto)));
    }
    
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] NewsDTO newsDto)
    {
        return HandleResult(await Mediator.Send(new UpdateNewsCommand(newsDto)));
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteNewsCommand(id)));
    }
}
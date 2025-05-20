using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.Create
{
    public record CreateNewsCommand(NewsDTO newNews) : IRequest<Result<NewsDTO>>;
}

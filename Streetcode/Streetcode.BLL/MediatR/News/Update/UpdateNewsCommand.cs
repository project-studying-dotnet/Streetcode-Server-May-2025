using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.DAL.Entities.News;

namespace Streetcode.BLL.MediatR.News.Update
{
    public record UpdateNewsCommand(NewsDTO news) : IRequest<Result<NewsDTO>>;
}

using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.GetAll;

public record GetAllNewsQuery() : IRequest<Result<IEnumerable<NewsDTO>>>;
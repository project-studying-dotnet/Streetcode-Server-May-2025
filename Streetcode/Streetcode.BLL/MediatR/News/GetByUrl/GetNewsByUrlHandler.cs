using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.News;

namespace Streetcode.BLL.MediatR.News.GetByUrl
{
    public class GetNewsByUrlHandler : IRequestHandler<GetNewsByUrlQuery, Result<NewsDTO>>
    {
        private readonly ILoggerService _logger;
        private readonly INewsService _newsService;
        public GetNewsByUrlHandler(ILoggerService logger, INewsService newsService)
        {
            _newsService = newsService;
            _logger = logger;
        }

        public async Task<Result<NewsDTO>> Handle(GetNewsByUrlQuery request, CancellationToken cancellationToken)
        {
            string url = request.url;
            var newsDto = await _newsService.GetNewsByUrlAsync(url);

            if(newsDto is null)
            {
                string errorMsg = $"No news by entered Url - {url}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            return Result.Ok(newsDto);
        }
    }
}
using FluentResults;
using MediatR;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.News;

namespace Streetcode.BLL.MediatR.News.GetByUrl;

public class GetNewsByUrlHandler : IRequestHandler<GetNewsByUrlQuery, Result<NewsDTO>>
{
    private readonly ILoggerService _logger;
    private readonly INewsService _newsService;
    private readonly IStringLocalizer<GetNewsByUrlHandler> _localizer;
    public GetNewsByUrlHandler(ILoggerService logger,
        INewsService newsService,
        IStringLocalizer<GetNewsByUrlHandler> localizer)
    {
        _newsService = newsService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<NewsDTO>> Handle(GetNewsByUrlQuery request, CancellationToken cancellationToken)
    {
        string url = request.url;
        var newsDto = await _newsService.GetNewsByUrlAsync(url);

        if (newsDto is null)
        {
            var errorMsg = _localizer["NoNewsByEnteredUrl", url];
            _logger.LogError(request, errorMsg);

            return Result.Fail(errorMsg);
        }

        return Result.Ok(newsDto);
    }
}
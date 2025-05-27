using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.News;

namespace Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;

public class GetNewsAndLinksByUrlHandler : IRequestHandler<GetNewsAndLinksByUrlQuery, Result<NewsDTOWithURLs>>
{
    private readonly ILoggerService _logger;
    private readonly INewsService _newsService;

    public GetNewsAndLinksByUrlHandler(INewsService newsService, ILoggerService logger)
    {
        _logger = logger;
        _newsService = newsService;
    }

    public async Task<Result<NewsDTOWithURLs>> Handle(GetNewsAndLinksByUrlQuery request, CancellationToken cancellationToken)
    {
        string url = request.url;
        var newsDTOWithUrls = await _newsService.GetNewsWithURLsAsync(url);

        if (newsDTOWithUrls == null)
        {
            string errorMsg = $"No news by entered Url - {url}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        return Result.Ok(newsDTOWithUrls);
    }
}
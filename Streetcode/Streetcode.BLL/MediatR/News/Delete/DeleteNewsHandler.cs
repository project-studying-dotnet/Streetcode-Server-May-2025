using FluentResults;
using MediatR;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.News.Delete;

public class DeleteNewsHandler : IRequestHandler<DeleteNewsCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IStringLocalizer<DeleteNewsHandler> _localizer;

    public DeleteNewsHandler(IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IStringLocalizer<DeleteNewsHandler> localizer)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<Unit>> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
    {
        int id = request.id;
        var news = await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(n => n.Id == id);
        if (news == null)
        {
            var errorMsg = _localizer["NoNewsFoundById", id];
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        if (news.Image is not null)
        {
            _repositoryWrapper.ImageRepository.Delete(news.Image);
        }

        _repositoryWrapper.NewsRepository.Delete(news);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            var errorMsg = _localizer["FailedToDeleteNews"];
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
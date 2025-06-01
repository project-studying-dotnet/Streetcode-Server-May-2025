using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.News.Create;

public class CreateNewsHandler : IRequestHandler<CreateNewsCommand, Result<NewsDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IStringLocalizer<CreateNewsHandler> _localizer;

    public CreateNewsHandler(IMapper mapper,
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger, 
        IStringLocalizer<CreateNewsHandler> localizer)
    {
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<NewsDTO>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
    {
        var newNews = _mapper.Map<DAL.Entities.News.News>(request.newNews);
        if (newNews is null)
        {
            var errorMsg = _localizer["CannotConvertNullToNews"];
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        if (newNews.ImageId == 0)
        {
            newNews.ImageId = null;
        }

        var entity = await _repositoryWrapper.NewsRepository.CreateAsync(newNews);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            return Result.Ok(_mapper.Map<NewsDTO>(entity));
        }
        else
        {
            var errorMsg = _localizer["FailedToCreateNews"];
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
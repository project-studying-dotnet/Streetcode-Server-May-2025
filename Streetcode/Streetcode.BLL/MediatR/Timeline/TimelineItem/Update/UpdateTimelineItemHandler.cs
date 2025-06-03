using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
public class UpdateTimelineItemHandler : IRequestHandler<UpdateTimelineItemCommand, Result<TimelineItemDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;
    public UpdateTimelineItemHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TimelineItemDTO>> Handle(UpdateTimelineItemCommand request, CancellationToken cancellationToken)
    {
        var timelineItem = await _repositoryWrapper.TimelineRepository.GetFirstOrDefaultAsync(x => x.Id == request.Id);
        if (timelineItem is null)
        {
            const string errorMsg = "Cannot find TimelineItem";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        _mapper.Map(request.TimelineItem, timelineItem);

        _repositoryWrapper.TimelineRepository.Update(timelineItem);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            return Result.Ok(request.TimelineItem);
        }
        else
        {
            const string errorMsg = "Failed to update TimelineItem";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}
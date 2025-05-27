using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;

public class DeleteTimelineItemHandler : IRequestHandler<DeleteTimelineItemCommand, Result<TimelineItemDTO>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public DeleteTimelineItemHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TimelineItemDTO>> Handle(DeleteTimelineItemCommand request, CancellationToken cancellationToken)
    {
        var timelineItem = await _repository.TimelineRepository.GetFirstOrDefaultAsync(t => t.Id == request.Id);

        if (timelineItem is null)
        {
            const string errorMsg = "Timeline item not found";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        _repository.TimelineRepository.Delete(timelineItem);
        var result = await _repository.SaveChangesAsync();

        if (result == 0)
        {
            const string errorMsg = "Failed to delete timeline item.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        return Result.Ok(_mapper.Map<TimelineItemDTO>(timelineItem));
    }
}
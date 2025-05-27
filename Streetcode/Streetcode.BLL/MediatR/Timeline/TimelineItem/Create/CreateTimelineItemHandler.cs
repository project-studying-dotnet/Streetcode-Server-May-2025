using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;

public class CreateTimelineItemHandler : IRequestHandler<CreateTimelineItemQuery, Result<TimelineItemDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public CreateTimelineItemHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TimelineItemDTO>> Handle(CreateTimelineItemQuery request, CancellationToken cancellationToken)
    {
        var newTimelineItem = _mapper.Map<TimelineItemEntity>(request.newTimelineItem);

        if (newTimelineItem == null)
        {
            string errorMsg = "Ivalid timeline item provided";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        if (newTimelineItem.StreetcodeId <= 0)
        {
            string errorMsg = "StreetcodeId must be a positive number";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var streetcodeExists = await _repositoryWrapper.StreetcodeRepository
            .GetFirstOrDefaultAsync(s => s.Id == newTimelineItem.StreetcodeId);

        if (streetcodeExists == null)
        {
            string errorMsg = $"Streetcode with id {newTimelineItem.StreetcodeId} does not exist";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        if (request.newTimelineItem.HistoricalContexts != null && request.newTimelineItem.HistoricalContexts.Any())
        {
            var contextIds = request.newTimelineItem.HistoricalContexts.Select(h => h.Id);

            foreach (var contextId in contextIds)
            {
                newTimelineItem.HistoricalContextTimelines.Add(new HistoricalContextTimeline
                {
                    HistoricalContextId = contextId,
                    TimelineId = newTimelineItem.Id
                });
            }
        }

        _repositoryWrapper.TimelineRepository.Create(newTimelineItem);
        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            return Result.Ok(_mapper.Map<TimelineItemDTO>(newTimelineItem));
        }
        else
        {
            const string errorMsg = "Failed to save timeline item.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}

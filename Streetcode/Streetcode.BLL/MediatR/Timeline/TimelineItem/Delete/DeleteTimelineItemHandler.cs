using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete
{
    public class DeleteTimelineItemHandler : IRequestHandler<DeleteTimelineItemCommand, Result<TimelineItemDTO>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;

        public DeleteTimelineItemHandler(IRepositoryWrapper repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<TimelineItemDTO>> Handle(DeleteTimelineItemCommand request, CancellationToken cancellationToken)
        {
            var timelineItem = await _repository.TimelineRepository.GetFirstOrDefaultAsync(t => t.Id == request.Id);

            if (timelineItem is null)
            {
                return Result.Fail($"Timeline item with id {request.Id} not found.");
            }

            _repository.TimelineRepository.Delete(timelineItem);
            var result = await _repository.SaveChangesAsync();

            if (result == 0)
            {
                return Result.Fail("Failed to delete timeline item.");
            }

            var deletedDto = _mapper.Map<TimelineItemDTO>(timelineItem);
            return Result.Ok(deletedDto);
        }
    }
}

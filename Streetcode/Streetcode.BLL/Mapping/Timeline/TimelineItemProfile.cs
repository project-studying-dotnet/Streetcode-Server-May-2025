using AutoMapper;
using Streetcode.BLL.DTO.Timeline;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Mapping.Timeline;

public class TimelineItemProfile : Profile
{
    public TimelineItemProfile()
    {
        CreateMap<TimelineItem, TimelineItemDTO>().ReverseMap();

        CreateMap<TimelineItem, TimelineItemDTO>()
            .ForMember(dest => dest.HistoricalContexts, opt => opt.MapFrom(x => x.HistoricalContextTimelines
                .Select(x => new HistoricalContextDTO
                {
                    Id = x.HistoricalContextId,
                    Title = x.HistoricalContext != null
                        ? (x.HistoricalContext.Title ?? string.Empty)
                        : string.Empty
                }).ToList()));
    }
}
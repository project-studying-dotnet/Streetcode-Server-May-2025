using AutoMapper;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Partners;
using Streetcode.DAL.Entities.Partners;

namespace Streetcode.BLL.Mapping.Partners;

public class PartnerProfile : Profile
{
    public PartnerProfile()
    {
        CreateMap<Partner, PartnerDTO>()
            .ForMember(dto => dto.TargetUrl, opt =>
                opt.MapFrom(src => (string.IsNullOrEmpty(src.TargetUrl) && string.IsNullOrEmpty(src.UrlTitle)) ? null : new UrlDTO
                {
                    Title = src.UrlTitle, Href = src.TargetUrl ?? string.Empty
                }))
            .ReverseMap()
            .ForMember(dest => dest.UrlTitle, opt => opt.MapFrom(src => src.TargetUrl!.Title))
            .ForMember(dest => dest.TargetUrl, opt => opt.MapFrom(src => src.TargetUrl!.Href));

        CreateMap<Partner, CreatePartnerDTO>().ReverseMap();
        CreateMap<Partner, PartnerShortDTO>().ReverseMap();
    }
}
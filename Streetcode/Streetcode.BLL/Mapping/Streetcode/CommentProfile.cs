using AutoMapper;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.BLL.Mapping.Streetcode;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentDTO>()
            .ForMember(dest => dest.User, opt => opt
                .MapFrom(src => src.User))
            .ForMember(dest => dest.Replies, opt => opt
                .MapFrom(src => src.Replies));

        CreateMap<Comment, AdminCommentDTO>()
            .ForMember(dest => dest.StreetcodeName, opt => opt.MapFrom(src => src.Streetcode.Title));
    }
}
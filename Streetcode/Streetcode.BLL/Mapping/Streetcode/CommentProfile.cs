using AutoMapper;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Comment;
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
            .ForMember(dest => dest.User, opt => opt
                .MapFrom(src => src.User))
            .ForMember(dest => dest.Replies, opt => opt
                .MapFrom(src => src.Replies));

        CreateMap<CreateCommentDTO, Comment>()
            .ForMember(dest => dest.IsApproved, opt => opt
                .MapFrom(src => false)) 
            .ForMember(dest => dest.CreatedAt, opt => opt
                .MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.ParentCommentId, opt => opt
                .MapFrom(src => (int?)null))
            .ForMember(dest => dest.Replies, opt => opt
                .MapFrom(src => new List<Comment>()));
    }
}
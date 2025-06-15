using AutoMapper;
using UserService.WebApi.DTO.Messaging;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Mapping.Users;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RegisterUserDTO, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        CreateMap<User, UserResponseDTO>();

        CreateMap<User, UserRegisteredEventDTO>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}


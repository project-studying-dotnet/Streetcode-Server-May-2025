using AutoMapper;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;

namespace UserService.WebApi.Mapping.Users;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RegisterUserDTO, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();
    }
}


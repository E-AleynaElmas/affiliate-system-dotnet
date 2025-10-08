using AutoMapper;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.DTOs.Auth;

namespace AffiliateSystem.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.ReferredUsersCount, opt => opt.MapFrom(src => src.ReferredUsers.Count));

        CreateMap<User, UserInfo>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
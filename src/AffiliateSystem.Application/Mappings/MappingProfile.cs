using AutoMapper;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.DTOs.Admin;

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

        CreateMap<LoginAttempt, LoginAttemptDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.AttemptedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<ReferralLink, ReferralLinkDto>()
            .ForMember(dest => dest.FullUrl, opt => opt.MapFrom(src => src.GetFullUrl("https://yourdomain.com")));

        CreateMap<BlockedIp, BlockedIpDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                src.BlockedUntil == null || src.BlockedUntil > DateTime.UtcNow));
    }
}
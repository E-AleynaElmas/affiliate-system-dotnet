using AutoMapper;
using AffiliateSystem.Application.DTOs.Admin;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Extensions;
using AffiliateSystem.Domain.Interfaces;

namespace AffiliateSystem.Application.Services;

/// <summary>
/// Admin service implementation
/// </summary>
public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ReferralLink> _referralLinkRepository;
    private readonly ILoginAttemptRepository _loginAttemptRepository;
    private readonly IBlockedIpRepository _blockedIpRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminService(
        IRepository<User> userRepository,
        IRepository<ReferralLink> referralLinkRepository,
        ILoginAttemptRepository loginAttemptRepository,
        IBlockedIpRepository blockedIpRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _referralLinkRepository = referralLinkRepository;
        _loginAttemptRepository = loginAttemptRepository;
        _blockedIpRepository = blockedIpRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 10)
    {
        var users = await _userRepository.FindAsync(u => true);

        var pagedUsers = users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userDtos = _mapper.Map<List<UserDto>>(pagedUsers);
        return BaseResponse<List<UserDto>>.SuccessResponse(userDtos, "Users retrieved successfully");
    }

    public async Task<BaseResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<UserDto>.ErrorResponse("User not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return BaseResponse<UserDto>.SuccessResponse(userDto, "User retrieved successfully");
    }

    public async Task<BaseResponse<bool>> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<bool>.ErrorResponse("User not found");
        }

        _userRepository.Remove(user);
        await _unitOfWork.CompleteAsync();

        return BaseResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }

    public async Task<BaseResponse<AdminStatisticsDto>> GetStatisticsAsync()
    {
        var allUsers = await _userRepository.FindAsync(u => true);
        var allReferralLinks = await _referralLinkRepository.FindAsync(r => true);
        var allLoginAttempts = await _loginAttemptRepository.FindAsync(l => true);
        var allBlockedIps = await _blockedIpRepository.GetAllAsync();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var thisWeekStart = today.AddDays(-(int)now.DayOfWeek);
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);

        var statistics = new AdminStatisticsDto
        {
            TotalUsers = allUsers.Count(),
            ActiveUsers = allUsers.Count(u => u.IsActive),
            BlockedUsers = allUsers.Count(u => !u.IsActive),
            TotalManagers = allUsers.Count(u => u.Role == Domain.Enums.UserRole.Manager),
            TotalCustomers = allUsers.Count(u => u.Role == Domain.Enums.UserRole.Customer),
            TotalReferralLinks = allReferralLinks.Count(),
            ActiveReferralLinks = allReferralLinks.Count(r => r.IsActive),
            TotalLoginAttempts = allLoginAttempts.Count(),
            FailedLoginAttempts = allLoginAttempts.Count(l => !l.IsSuccessful),
            BlockedIpCount = allBlockedIps.Count(b => b.BlockedUntil == null || b.BlockedUntil > now),
            UsersRegisteredToday = allUsers.Count(u => u.CreatedAt >= today),
            UsersRegisteredThisWeek = allUsers.Count(u => u.CreatedAt >= thisWeekStart),
            UsersRegisteredThisMonth = allUsers.Count(u => u.CreatedAt >= thisMonthStart)
        };

        return BaseResponse<AdminStatisticsDto>.SuccessResponse(statistics, "Statistics retrieved successfully");
    }

    public async Task<BaseResponse<List<BlockedIpDto>>> GetBlockedIpsAsync()
    {
        var blockedIps = await _blockedIpRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var blockedIpDtos = blockedIps.Select(b => new BlockedIpDto
        {
            IpAddress = b.IpAddress,
            CreatedAt = b.CreatedAt,
            BlockedUntil = b.BlockedUntil,
            FailedAttemptCount = b.FailedAttemptCount,
            IsManualBlock = b.IsManualBlock,
            IsActive = b.BlockedUntil == null || b.BlockedUntil > now
        }).ToList();

        return BaseResponse<List<BlockedIpDto>>.SuccessResponse(blockedIpDtos, "Blocked IPs retrieved successfully");
    }

    public async Task<BaseResponse<bool>> UnblockIpAsync(string ipAddress)
    {
        var blockedIp = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (blockedIp == null)
        {
            return BaseResponse<bool>.ErrorResponse("IP address not found in blocked list");
        }

        _blockedIpRepository.Remove(blockedIp);
        await _unitOfWork.CompleteAsync();

        return BaseResponse<bool>.SuccessResponse(true, "IP address unblocked successfully");
    }

    public async Task<BaseResponse<bool>> BlockIpAsync(string ipAddress, int? durationHours = null)
    {
        var existingBlock = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (existingBlock != null)
        {
            return BaseResponse<bool>.ErrorResponse("IP address is already blocked");
        }

        var blockedIp = new BlockedIp
        {
            IpAddress = ipAddress,
            BlockedUntil = durationHours.HasValue ? DateTime.UtcNow.AddHours(durationHours.Value) : null,
            FailedAttemptCount = 0,
            IsManualBlock = true
        };

        await _blockedIpRepository.AddAsync(blockedIp);
        await _unitOfWork.CompleteAsync();

        return BaseResponse<bool>.SuccessResponse(true, "IP address blocked successfully");
    }
}

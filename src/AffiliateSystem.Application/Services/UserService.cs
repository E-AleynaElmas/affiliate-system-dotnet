using AutoMapper;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Domain.Interfaces;

namespace AffiliateSystem.Application.Services;

/// <summary>
/// User management service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<LoginAttempt> _loginAttemptRepository;
    private readonly IRepository<ReferralLink> _referralLinkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UserService(
        IRepository<User> userRepository,
        IRepository<LoginAttempt> loginAttemptRepository,
        IRepository<ReferralLink> referralLinkRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _loginAttemptRepository = loginAttemptRepository;
        _referralLinkRepository = referralLinkRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<BaseResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<UserDto>.ErrorResponse("User not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return BaseResponse<UserDto>.SuccessResponse(userDto);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    public async Task<BaseResponse<UserDto>> GetCurrentUserAsync(Guid currentUserId)
    {
        return await GetUserByIdAsync(currentUserId);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    public async Task<BaseResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<UserDto>.ErrorResponse("User not found");
        }

        // Update only allowed fields
        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync();

        var userDto = _mapper.Map<UserDto>(user);
        return BaseResponse<UserDto>.SuccessResponse(userDto, "Profile updated successfully");
    }

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<BaseResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<bool>.ErrorResponse("User not found");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            return BaseResponse<bool>.ErrorResponse("Current password is incorrect");
        }

        // Hash new password
        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync();

        return BaseResponse<bool>.SuccessResponse(true, "Password changed successfully");
    }

    /// <summary>
    /// Get user dashboard data
    /// </summary>
    public async Task<BaseResponse<DashboardDto>> GetDashboardAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<DashboardDto>.ErrorResponse("User not found");
        }

        var dashboard = new DashboardDto
        {
            TotalReferrals = 0,
            ActiveReferralLinks = 0,
            RecentLoginAttempts = new List<LoginAttemptDto>(),
            ReferralLinks = new List<ReferralLinkDto>()
        };

        // Get referral statistics for managers
        if (user.Role == UserRole.Manager || user.Role == UserRole.Admin)
        {
            // Count users referred by this user
            var referredUsers = await _userRepository.FindAsync(u => u.ReferredById == userId);
            dashboard.TotalReferrals = referredUsers.Count();

            // Get referral links
            var referralLinks = await _referralLinkRepository.FindAsync(r => r.CreatedByUserId == userId);
            dashboard.ActiveReferralLinks = referralLinks.Count(r => r.IsActive && r.CanBeUsed());

            dashboard.ReferralLinks = referralLinks.Select(r => new ReferralLinkDto
            {
                Id = r.Id,
                Code = r.Code,
                UsageCount = r.UsageCount,
                MaxUsages = r.MaxUsages,
                ExpiresAt = r.ExpiresAt,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                FullUrl = r.GetFullUrl("https://yourdomain.com") // Will be configurable later
            }).ToList();
        }

        // Get recent login attempts
        var allLoginAttempts = await _loginAttemptRepository.FindAsync(
            a => a.UserId == userId);
        var loginAttempts = allLoginAttempts
            .OrderByDescending(a => a.CreatedAt)
            .Take(10);

        dashboard.RecentLoginAttempts = loginAttempts.Select(a => new LoginAttemptDto
        {
            IpAddress = a.IpAddress,
            IsSuccessful = a.IsSuccessful,
            UserAgent = a.UserAgent,
            AttemptedAt = a.AttemptedAt,
            FailureReason = a.FailureReason
        }).ToList();

        return BaseResponse<DashboardDto>.SuccessResponse(dashboard);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    public async Task<BaseResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return BaseResponse<IEnumerable<UserDto>>.SuccessResponse(userDtos);
    }

    /// <summary>
    /// Activate or deactivate user (Admin only)
    /// </summary>
    public async Task<BaseResponse<bool>> SetUserActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<bool>.ErrorResponse("User not found");
        }

        user.IsActive = isActive;
        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync();

        var status = isActive ? "activated" : "deactivated";
        return BaseResponse<bool>.SuccessResponse(true, $"User {status} successfully");
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
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

    /// <summary>
    /// Create referral link for user
    /// </summary>
    public async Task<BaseResponse<ReferralLinkDto>> CreateReferralLinkAsync(Guid userId, CreateReferralLinkRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return BaseResponse<ReferralLinkDto>.ErrorResponse("User not found");
        }

        // Only managers and admins can create referral links
        if (user.Role != UserRole.Manager && user.Role != UserRole.Admin)
        {
            return BaseResponse<ReferralLinkDto>.ErrorResponse("Only managers and admins can create referral links");
        }

        // Generate secure random code
        var code = GenerateSecureReferralCode();

        // Ensure code is unique
        while (await _referralLinkRepository.AnyAsync(r => r.Code == code))
        {
            code = GenerateSecureReferralCode();
        }

        var referralLink = new ReferralLink
        {
            Code = code,
            CreatedByUserId = userId,
            MaxUsages = request.MaxUsages,
            ExpiresAt = request.ExpiresAt,
            IsActive = true
        };

        await _referralLinkRepository.AddAsync(referralLink);
        await _unitOfWork.CompleteAsync();

        var dto = new ReferralLinkDto
        {
            Id = referralLink.Id,
            Code = referralLink.Code,
            UsageCount = 0,
            MaxUsages = referralLink.MaxUsages,
            ExpiresAt = referralLink.ExpiresAt,
            IsActive = referralLink.IsActive,
            CreatedAt = referralLink.CreatedAt,
            FullUrl = referralLink.GetFullUrl("https://yourdomain.com")
        };

        return BaseResponse<ReferralLinkDto>.SuccessResponse(dto, "Referral link created successfully");
    }

    /// <summary>
    /// Generate cryptographically secure referral code
    /// </summary>
    private string GenerateSecureReferralCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var code = new char[8];

        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            var bytes = new byte[8];
            rng.GetBytes(bytes);

            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[bytes[i] % chars.Length];
            }
        }

        return new string(code);
    }
}
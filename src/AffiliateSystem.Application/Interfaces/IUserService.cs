using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.DTOs.Common;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<BaseResponse<UserDto>> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Get user dashboard data
    /// </summary>
    Task<BaseResponse<DashboardDto>> GetDashboardAsync(Guid userId);

    /// <summary>
    /// Get all users (admin only)
    /// </summary>
    Task<BaseResponse<IEnumerable<UserDto>>> GetAllUsersAsync();

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<BaseResponse<UserDto>> UpdateUserAsync(Guid userId, UserDto userDto);

    /// <summary>
    /// Deactivate user
    /// </summary>
    Task<BaseResponse<bool>> DeactivateUserAsync(Guid userId);

    /// <summary>
    /// Generate referral code for user
    /// </summary>
    Task<BaseResponse<string>> GenerateReferralCodeAsync(Guid userId);
}
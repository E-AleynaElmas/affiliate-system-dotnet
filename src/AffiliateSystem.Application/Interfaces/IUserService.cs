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
    Task<BaseResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<BaseResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    /// <summary>
    /// Create referral link for user
    /// </summary>
    Task<BaseResponse<ReferralLinkDto>> CreateReferralLinkAsync(Guid userId, CreateReferralLinkRequest request);
}
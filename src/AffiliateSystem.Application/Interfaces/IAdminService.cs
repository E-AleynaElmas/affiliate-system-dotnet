using AffiliateSystem.Application.DTOs.Admin;
using AffiliateSystem.Application.DTOs.Common;
using AffiliateSystem.Application.DTOs.User;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Service for admin operations
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    Task<BaseResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<BaseResponse<UserDto>> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Delete user by ID
    /// </summary>
    Task<BaseResponse<bool>> DeleteUserAsync(Guid userId);

    /// <summary>
    /// Get system statistics
    /// </summary>
    Task<BaseResponse<AdminStatisticsDto>> GetStatisticsAsync();

    /// <summary>
    /// Get all blocked IPs
    /// </summary>
    Task<BaseResponse<List<BlockedIpDto>>> GetBlockedIpsAsync();

    /// <summary>
    /// Unblock an IP address
    /// </summary>
    Task<BaseResponse<bool>> UnblockIpAsync(string ipAddress);

    /// <summary>
    /// Manually block an IP address
    /// </summary>
    Task<BaseResponse<bool>> BlockIpAsync(string ipAddress, int? durationHours = null);
}

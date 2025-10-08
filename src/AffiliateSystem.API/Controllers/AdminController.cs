using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Infrastructure.Filters;

namespace AffiliateSystem.API.Controllers;

/// <summary>
/// Admin controller for system management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of users</returns>
    [HttpGet("users")]
    [MonitorPerformance]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Admin fetching all users - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var result = await _adminService.GetAllUsersAsync(page, pageSize);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        _logger.LogInformation("Admin fetching user by ID: {UserId}", id);

        var result = await _adminService.GetUserByIdAsync(id);
        return ToActionResultWithNotFound(result);
    }

    /// <summary>
    /// Delete user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogWarning("Admin attempting to delete user: {UserId}", id);

        var result = await _adminService.DeleteUserAsync(id);

        if (result.Success)
        {
            _logger.LogInformation("User deleted successfully: {UserId}", id);
        }

        return ToActionResultWithNotFound(result);
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    /// <returns>Admin dashboard statistics</returns>
    [HttpGet("statistics")]
    [MonitorPerformance]
    public async Task<IActionResult> GetStatistics()
    {
        _logger.LogInformation("Admin fetching system statistics");

        var result = await _adminService.GetStatisticsAsync();
        return ToActionResult(result);
    }

    /// <summary>
    /// Get all blocked IPs
    /// </summary>
    /// <returns>List of blocked IP addresses</returns>
    [HttpGet("blocked-ips")]
    public async Task<IActionResult> GetBlockedIps()
    {
        _logger.LogInformation("Admin fetching blocked IPs");

        var result = await _adminService.GetBlockedIpsAsync();
        return ToActionResult(result);
    }

    /// <summary>
    /// Unblock an IP address
    /// </summary>
    /// <param name="ipAddress">IP address to unblock</param>
    /// <returns>Success status</returns>
    [HttpDelete("blocked-ips/{ipAddress}")]
    public async Task<IActionResult> UnblockIp(string ipAddress)
    {
        _logger.LogInformation("Admin unblocking IP: {IpAddress}", ipAddress);

        var result = await _adminService.UnblockIpAsync(ipAddress);

        if (result.Success)
        {
            _logger.LogInformation("IP unblocked successfully: {IpAddress}", ipAddress);
        }

        return ToActionResultWithNotFound(result);
    }

    /// <summary>
    /// Manually block an IP address
    /// </summary>
    /// <param name="ipAddress">IP address to block</param>
    /// <param name="durationHours">Duration in hours (null for permanent)</param>
    /// <returns>Success status</returns>
    [HttpPost("blocked-ips/{ipAddress}")]
    public async Task<IActionResult> BlockIp(string ipAddress, [FromQuery] int? durationHours = null)
    {
        _logger.LogWarning("Admin manually blocking IP: {IpAddress}, Duration: {Duration} hours", ipAddress, durationHours ?? 0);

        var result = await _adminService.BlockIpAsync(ipAddress, durationHours);

        if (result.Success)
        {
            _logger.LogInformation("IP blocked successfully: {IpAddress}", ipAddress);
        }

        return ToActionResult(result);
    }
}

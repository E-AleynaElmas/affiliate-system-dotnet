using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.Interfaces;
using System.Security.Claims;

namespace AffiliateSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetUserByIdAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { message = "An error occurred while getting profile" });
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UpdateUserAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.ChangePasswordAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetDashboardAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard");
            return StatusCode(500, new { message = "An error occurred while getting dashboard" });
        }
    }

    [HttpPost("referral-link")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CreateReferralLink([FromBody] CreateReferralLinkRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.CreateReferralLinkAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating referral link");
            return StatusCode(500, new { message = "An error occurred while creating referral link" });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var result = await _userService.GetAllUsersAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, new { message = "An error occurred while getting users" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while getting user" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }
}

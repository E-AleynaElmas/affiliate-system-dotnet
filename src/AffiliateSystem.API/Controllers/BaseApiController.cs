using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AffiliateSystem.Application.DTOs.Common;

namespace AffiliateSystem.API.Controllers;

/// <summary>
/// Base controller with common functionality for all API controllers
/// </summary>
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Convert service response to appropriate HTTP action result
    /// </summary>
    protected IActionResult ToActionResult<T>(BaseResponse<T> response)
    {
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Convert service response to HTTP action result with NotFound support
    /// </summary>
    protected IActionResult ToActionResultWithNotFound<T>(BaseResponse<T> response)
    {
        if (!response.Success && response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(response);
        }

        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Get current authenticated user ID from JWT token
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    /// <summary>
    /// Get current authenticated user email from JWT token
    /// </summary>
    protected string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedAccessException("Email not found in token");
    }

    /// <summary>
    /// Get current authenticated user role from JWT token
    /// </summary>
    protected string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("Role not found in token");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.Interfaces;

namespace AffiliateSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetUserByIdAsync(userId);
        return ToActionResult(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.UpdateUserAsync(userId, request);
        return ToActionResult(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.ChangePasswordAsync(userId, request);
        return ToActionResult(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetDashboardAsync(userId);
        return ToActionResult(result);
    }

    [HttpPost("referral-link")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CreateReferralLink([FromBody] CreateReferralLinkRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.CreateReferralLinkAsync(userId, request);
        return ToActionResult(result);
    }
}

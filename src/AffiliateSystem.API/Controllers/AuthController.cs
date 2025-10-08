using Microsoft.AspNetCore.Mvc;
using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Infrastructure.Middleware;

namespace AffiliateSystem.API.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Get client IP address from middleware
            request.IpAddress = HttpContext.GetClientIpAddress();
            request.UserAgent = HttpContext.GetUserAgent();

            _logger.LogInformation("Login attempt from IP: {IpAddress} for email: {Email}",
                request.IpAddress, request.Email);

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Successful login for email: {Email}", request.Email);
                return Ok(result);
            }

            _logger.LogWarning("Failed login attempt for email: {Email}. Reason: {Message}",
                request.Email, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// User registration endpoint
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration response with auto-login</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Successful registration for email: {Email}", request.Email);
                return Ok(result);
            }

            _logger.LogWarning("Failed registration for email: {Email}. Reason: {Message}",
                request.Email, result.Message);
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Validate referral code
    /// </summary>
    /// <param name="code">Referral code to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    [HttpGet("validate-referral/{code}")]
    public async Task<IActionResult> ValidateReferralCode(string code)
    {
        try
        {
            var isValid = await _authService.ValidateReferralCodeAsync(code);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating referral code: {Code}", code);
            return StatusCode(500, new { message = "An error occurred while validating referral code" });
        }
    }

    /// <summary>
    /// Check if an IP address is blocked
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <returns>True if blocked, false otherwise</returns>
    [HttpGet("check-ip/{ipAddress}")]
    public async Task<IActionResult> CheckIpStatus(string ipAddress)
    {
        try
        {
            var isBlocked = await _authService.IsIpBlockedAsync(ipAddress);
            return Ok(new { isBlocked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking IP status: {IpAddress}", ipAddress);
            return StatusCode(500, new { message = "An error occurred while checking IP status" });
        }
    }
}
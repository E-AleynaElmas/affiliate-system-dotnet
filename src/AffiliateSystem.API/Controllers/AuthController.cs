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
public class AuthController : BaseApiController
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
        request.IpAddress = HttpContext.GetClientIpAddress();
        request.UserAgent = HttpContext.GetUserAgent();

        _logger.LogInformation("Login attempt: {Email} from {IpAddress}", request.Email, request.IpAddress);

        var result = await _authService.LoginAsync(request);

        if (result.Success)
        {
            _logger.LogInformation("Login successful: {Email}", request.Email);
        }
        else
        {
            _logger.LogWarning("Login failed: {Email} - {Message}", request.Email, result.Message);
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// User registration endpoint
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration response with auto-login</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt: {Email}", request.Email);

        var result = await _authService.RegisterAsync(request);

        if (result.Success)
        {
            _logger.LogInformation("Registration successful: {Email}", request.Email);
        }
        else
        {
            _logger.LogWarning("Registration failed: {Email} - {Message}", request.Email, result.Message);
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Validate referral code
    /// </summary>
    /// <param name="code">Referral code to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    [HttpGet("validate-referral/{code}")]
    public async Task<IActionResult> ValidateReferralCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "Referral code is required" });
        }

        var isValid = await _authService.ValidateReferralCodeAsync(code);
        return Ok(new ValidateReferralResponse
        {
            IsValid = isValid,
            Message = isValid ? "Referral code is valid" : "Referral code is invalid or expired"
        });
    }

    /// <summary>
    /// Check if an IP address is blocked
    /// </summary>
    /// <param name="ipAddress">IP address to check</param>
    /// <returns>True if blocked, false otherwise</returns>
    [HttpGet("check-ip/{ipAddress}")]
    public async Task<IActionResult> CheckIpStatus(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return BadRequest(new { message = "IP address is required" });
        }

        var isBlocked = await _authService.IsIpBlockedAsync(ipAddress);
        return Ok(new CheckIpStatusResponse
        {
            IsBlocked = isBlocked,
            Message = isBlocked ? "IP address is currently blocked" : "IP address is not blocked"
        });
    }
}

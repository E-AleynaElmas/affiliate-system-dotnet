using AffiliateSystem.Application.DTOs.Auth;
using AffiliateSystem.Application.DTOs.Common;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// User login
    /// </summary>
    Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// User registration
    /// </summary>
    Task<BaseResponse<LoginResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Validate referral code
    /// </summary>
    Task<bool> ValidateReferralCodeAsync(string referralCode);

    /// <summary>
    /// Check if IP is blocked
    /// </summary>
    Task<bool> IsIpBlockedAsync(string ipAddress);
}
namespace AffiliateSystem.Application.DTOs.Auth;

/// <summary>
/// Response for referral code validation
/// </summary>
public class ValidateReferralResponse
{
    /// <summary>
    /// Indicates whether the referral code is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Optional message providing additional information
    /// </summary>
    public string? Message { get; set; }
}

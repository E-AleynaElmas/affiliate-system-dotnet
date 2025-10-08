namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// Request DTO for creating a referral link
/// </summary>
public class CreateReferralLinkRequest
{
    /// <summary>
    /// Maximum number of times the link can be used (optional)
    /// </summary>
    public int? MaxUsages { get; set; }

    /// <summary>
    /// Expiration date for the link (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Custom code for the link (optional, will be auto-generated if not provided)
    /// </summary>
    public string? CustomCode { get; set; }
}
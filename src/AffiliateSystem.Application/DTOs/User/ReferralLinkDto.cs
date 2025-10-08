namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// DTO for referral link information
/// </summary>
public class ReferralLinkDto
{
    /// <summary>
    /// Unique identifier for the referral link
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Referral code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full URL for the referral link
    /// </summary>
    public string FullUrl { get; set; } = string.Empty;

    /// <summary>
    /// Number of times the link has been used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Maximum number of times the link can be used
    /// </summary>
    public int? MaxUsages { get; set; }

    /// <summary>
    /// Expiration date for the link
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Is the link currently active?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date when the link was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
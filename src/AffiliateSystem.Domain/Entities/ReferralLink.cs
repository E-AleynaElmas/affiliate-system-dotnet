namespace AffiliateSystem.Domain.Entities;

/// <summary>
/// Entity for managing referral links
/// Provides protection against brute force attacks
/// </summary>
public class ReferralLink : BaseEntity
{
    /// <summary>
    /// Unique referral code
    /// Must be cryptographically secure random string
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// User ID who created the link
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// User who created the link (Navigation Property)
    /// </summary>
    public virtual User CreatedBy { get; set; } = null!;

    /// <summary>
    /// Usage count of the link
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Maximum usage count
    /// null means unlimited
    /// </summary>
    public int? MaxUsages { get; set; }

    /// <summary>
    /// Link expiration date
    /// null means no expiration
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Is the link active?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Get the full URL of the link
    /// </summary>
    public string GetFullUrl(string baseUrl)
    {
        return $"{baseUrl}/register?ref={Code}";
    }

    /// <summary>
    /// Can the link be used?
    /// </summary>
    public bool CanBeUsed()
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        if (MaxUsages.HasValue && UsageCount >= MaxUsages.Value) return false;
        return true;
    }

    public ReferralLink()
    {
        IsActive = true;
        UsageCount = 0;
    }
}
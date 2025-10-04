namespace AffiliateSystem.Domain.Entities;

/// <summary>
/// Entity for storing blocked IP addresses
/// IP is blocked after 10 failed login attempts
/// </summary>
public class BlockedIp : BaseEntity
{
    /// <summary>
    /// Blocked IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Reason for blocking
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Block expiration time
    /// null means permanent block
    /// </summary>
    public DateTime? BlockedUntil { get; set; }

    /// <summary>
    /// Number of failed attempts that triggered the block
    /// </summary>
    public int FailedAttemptCount { get; set; }

    /// <summary>
    /// Is the block active?
    /// </summary>
    public bool IsActive => BlockedUntil == null || BlockedUntil > DateTime.UtcNow;

    /// <summary>
    /// Was it manually blocked by admin?
    /// </summary>
    public bool IsManualBlock { get; set; }

    /// <summary>
    /// Admin user ID who unblocked (if manually unblocked)
    /// </summary>
    public Guid? UnblockedByUserId { get; set; }

    /// <summary>
    /// Date when the block was removed
    /// </summary>
    public DateTime? UnblockedAt { get; set; }
}
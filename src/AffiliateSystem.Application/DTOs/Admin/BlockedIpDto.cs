namespace AffiliateSystem.Application.DTOs.Admin;

/// <summary>
/// Blocked IP information for admin management
/// </summary>
public class BlockedIpDto
{
    /// <summary>
    /// Blocked IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Date when the IP was blocked
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the block expires (null for permanent blocks)
    /// </summary>
    public DateTime? BlockedUntil { get; set; }

    /// <summary>
    /// Number of failed attempts that led to the block
    /// </summary>
    public int FailedAttemptCount { get; set; }

    /// <summary>
    /// Whether this was a manual block by admin
    /// </summary>
    public bool IsManualBlock { get; set; }

    /// <summary>
    /// Whether the block is currently active
    /// </summary>
    public bool IsActive { get; set; }
}

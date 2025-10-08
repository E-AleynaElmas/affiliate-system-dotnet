namespace AffiliateSystem.Application.DTOs.Auth;

/// <summary>
/// Response for IP address blocking status check
/// </summary>
public class CheckIpStatusResponse
{
    /// <summary>
    /// Indicates whether the IP address is blocked
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Optional message providing additional information about the block
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// DateTime when the block will expire (if applicable)
    /// </summary>
    public DateTime? BlockedUntil { get; set; }
}

using AffiliateSystem.Application.DTOs.Common;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Service for managing IP blocking and failed login attempts
/// </summary>
public interface IIpBlockingService
{
    /// <summary>
    /// Check if an IP address is currently blocked
    /// </summary>
    Task<bool> IsBlockedAsync(string ipAddress);

    /// <summary>
    /// Block an IP address for a specified duration
    /// </summary>
    Task BlockIpAsync(string ipAddress, TimeSpan duration, string reason);

    /// <summary>
    /// Unblock an IP address
    /// </summary>
    Task UnblockIpAsync(string ipAddress);

    /// <summary>
    /// Get the number of failed attempts from an IP address
    /// </summary>
    Task<int> GetFailedAttemptsAsync(string ipAddress);

    /// <summary>
    /// Record a failed login attempt from an IP address
    /// </summary>
    Task RecordFailedAttemptAsync(string ipAddress, string? email = null);

    /// <summary>
    /// Clear failed attempts for an IP address (called on successful login)
    /// </summary>
    Task ClearFailedAttemptsAsync(string ipAddress);

    /// <summary>
    /// Get blocking information for an IP
    /// </summary>
    Task<IpBlockInfo?> GetBlockInfoAsync(string ipAddress);
}

/// <summary>
/// IP blocking information
/// </summary>
public class IpBlockInfo
{
    public string IpAddress { get; set; } = string.Empty;
    public DateTime BlockedAt { get; set; }
    public DateTime BlockedUntil { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
    public bool IsCurrentlyBlocked => BlockedUntil > DateTime.UtcNow;
}
using AffiliateSystem.Application.DTOs.User;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Service for tracking login attempts
/// </summary>
public interface ILoginAttemptService
{
    /// <summary>
    /// Record a login attempt
    /// </summary>
    Task RecordAttemptAsync(LoginAttemptDto attempt);

    /// <summary>
    /// Get recent login attempts for a user
    /// </summary>
    Task<IEnumerable<LoginAttemptDto>> GetRecentAttemptsAsync(Guid userId, int count = 10);

    /// <summary>
    /// Get failed attempts count for a user in a time window
    /// </summary>
    Task<int> GetFailedAttemptsCountAsync(Guid userId, int hoursWindow = 24);

    /// <summary>
    /// Get login attempts by IP address
    /// </summary>
    Task<IEnumerable<LoginAttemptDto>> GetAttemptsByIpAsync(string ipAddress, int count = 10);

    /// <summary>
    /// Get statistics for security dashboard
    /// </summary>
    Task<LoginAttemptStats> GetStatsAsync(int hoursWindow = 24);
}

/// <summary>
/// Login attempt statistics
/// </summary>
public class LoginAttemptStats
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulAttempts / TotalAttempts * 100 : 0;
    public Dictionary<string, int> TopFailedIps { get; set; } = new();
    public Dictionary<string, int> FailureReasons { get; set; } = new();
}
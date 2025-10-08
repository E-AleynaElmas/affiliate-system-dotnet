using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Domain.Interfaces;

/// <summary>
/// Repository interface for LoginAttempt entities
/// </summary>
public interface ILoginAttemptRepository : IRepository<LoginAttempt>
{
    /// <summary>
    /// Get recent login attempts by user ID
    /// </summary>
    Task<IEnumerable<LoginAttempt>> GetRecentByUserIdAsync(Guid userId, int count = 10);

    /// <summary>
    /// Get recent login attempts by IP address
    /// </summary>
    Task<IEnumerable<LoginAttempt>> GetRecentByIpAddressAsync(string ipAddress, int count = 10);

    /// <summary>
    /// Count failed attempts within time window
    /// </summary>
    Task<int> CountFailedAsync(int hoursWindow = 24);

    /// <summary>
    /// Get failed attempts by IP in time window
    /// </summary>
    Task<IEnumerable<LoginAttempt>> GetFailedByIpAsync(string ipAddress, int hoursWindow = 1);

    /// <summary>
    /// Clean up old login attempts
    /// </summary>
    Task<int> RemoveOldAttemptsAsync(int daysOld = 30);
}
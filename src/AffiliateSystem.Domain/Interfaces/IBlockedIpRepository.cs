using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Domain.Interfaces;

/// <summary>
/// Repository interface for BlockedIp entities
/// </summary>
public interface IBlockedIpRepository : IRepository<BlockedIp>
{
    /// <summary>
    /// Get blocked IP by IP address
    /// </summary>
    Task<BlockedIp?> GetByIpAddressAsync(string ipAddress);

    /// <summary>
    /// Count active blocks
    /// </summary>
    Task<int> CountActiveBlocksAsync();

    /// <summary>
    /// Get active blocked IPs
    /// </summary>
    Task<IEnumerable<BlockedIp>> GetActiveBlocksAsync();

    /// <summary>
    /// Clean up expired blocks
    /// </summary>
    Task<int> RemoveExpiredBlocksAsync();
}
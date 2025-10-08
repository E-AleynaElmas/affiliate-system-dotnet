using Microsoft.EntityFrameworkCore;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using AffiliateSystem.Infrastructure.Data;

namespace AffiliateSystem.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for BlockedIp entities
/// </summary>
public class BlockedIpRepository : Repository<BlockedIp>, IBlockedIpRepository
{
    public BlockedIpRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<BlockedIp?> GetByIpAddressAsync(string ipAddress)
    {
        return await _context.BlockedIps
            .FirstOrDefaultAsync(b => b.IpAddress == ipAddress);
    }

    public async Task<int> CountActiveBlocksAsync()
    {
        return await _context.BlockedIps
            .CountAsync(b => b.BlockedUntil > DateTime.UtcNow);
    }

    public async Task<IEnumerable<BlockedIp>> GetActiveBlocksAsync()
    {
        return await _context.BlockedIps
            .Where(b => b.BlockedUntil > DateTime.UtcNow)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> RemoveExpiredBlocksAsync()
    {
        var expiredBlocks = await _context.BlockedIps
            .Where(b => b.BlockedUntil <= DateTime.UtcNow)
            .ToListAsync();

        _context.BlockedIps.RemoveRange(expiredBlocks);
        await _context.SaveChangesAsync();

        return expiredBlocks.Count;
    }
}
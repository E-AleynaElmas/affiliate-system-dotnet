using Microsoft.EntityFrameworkCore;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using AffiliateSystem.Infrastructure.Data;

namespace AffiliateSystem.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LoginAttempt entities
/// </summary>
public class LoginAttemptRepository : Repository<LoginAttempt>, ILoginAttemptRepository
{
    public LoginAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LoginAttempt>> GetRecentByUserIdAsync(Guid userId, int count = 10)
    {
        return await _context.LoginAttempts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoginAttempt>> GetRecentByIpAddressAsync(string ipAddress, int count = 10)
    {
        return await _context.LoginAttempts
            .Where(a => a.IpAddress == ipAddress)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> CountFailedAsync(int hoursWindow = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hoursWindow);
        return await _context.LoginAttempts
            .CountAsync(a => !a.IsSuccessful && a.CreatedAt >= since);
    }

    public async Task<IEnumerable<LoginAttempt>> GetFailedByIpAsync(string ipAddress, int hoursWindow = 1)
    {
        var since = DateTime.UtcNow.AddHours(-hoursWindow);
        return await _context.LoginAttempts
            .Where(a => a.IpAddress == ipAddress && !a.IsSuccessful && a.CreatedAt >= since)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> RemoveOldAttemptsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldAttempts = await _context.LoginAttempts
            .Where(a => a.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.LoginAttempts.RemoveRange(oldAttempts);
        await _context.SaveChangesAsync();

        return oldAttempts.Count;
    }
}
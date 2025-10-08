using Microsoft.Extensions.Caching.Memory;
using AffiliateSystem.Application.Interfaces;

namespace AffiliateSystem.Infrastructure.Services;

/// <summary>
/// In-memory cache service implementation
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiry.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiry.Value;
        }
        else
        {
            options.SlidingExpiration = TimeSpan.FromMinutes(5); // Default sliding expiration
        }

        // Track key for pattern removal
        lock (_lock)
        {
            _keys.Add(key);
        }

        // Register callback to remove key from tracking when evicted
        options.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
        {
            lock (_lock)
            {
                _keys.Remove(evictedKey.ToString()!);
            }
        });

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);

        lock (_lock)
        {
            _keys.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiry);
        return value;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        List<string> keysToRemove;

        lock (_lock)
        {
            keysToRemove = _keys
                .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            lock (_lock)
            {
                _keys.Remove(key);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Cache key builder helper
/// </summary>
public static class CacheKeys
{
    // User cache keys
    public static string User(Guid userId) => $"user:{userId}";
    public static string UserByEmail(string email) => $"user:email:{email}";
    public static string UserDashboard(Guid userId) => $"user:dashboard:{userId}";

    // Referral cache keys
    public static string ReferralCode(string code) => $"referral:code:{code}";
    public static string UserReferralLinks(Guid userId) => $"referral:user:{userId}";

    // IP blocking cache keys
    public static string BlockedIp(string ipAddress) => $"blocked:ip:{ipAddress}";
    public static string LoginAttempts(string ipAddress) => $"attempts:ip:{ipAddress}";

    // Rate limiting cache keys
    public static string RateLimit(string ipAddress, string endpoint) => $"ratelimit:{endpoint}:{ipAddress}";
}
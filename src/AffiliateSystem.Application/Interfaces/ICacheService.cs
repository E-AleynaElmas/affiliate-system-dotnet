namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Caching service abstraction
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Set value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Get or set value with factory method
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);

    /// <summary>
    /// Remove all keys matching pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}
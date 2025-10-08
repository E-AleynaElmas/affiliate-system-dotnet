using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AffiliateSystem.Application.Configuration;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;

namespace AffiliateSystem.Infrastructure.Services;

/// <summary>
/// Service implementation for IP blocking and failed login attempt tracking
/// </summary>
public class IpBlockingService : IIpBlockingService
{
    private readonly ICacheService _cache;
    private readonly IBlockedIpRepository _blockedIpRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IpBlockingService> _logger;
    private readonly SecuritySettings _settings;

    public IpBlockingService(
        ICacheService cache,
        IBlockedIpRepository blockedIpRepository,
        IUnitOfWork unitOfWork,
        ILogger<IpBlockingService> logger,
        IOptions<SecuritySettings> settings)
    {
        _cache = cache;
        _blockedIpRepository = blockedIpRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<bool> IsBlockedAsync(string ipAddress)
    {
        // Check cache first for performance
        var cacheKey = $"blocked_ip:{ipAddress}";
        var cachedResult = await _cache.GetAsync<bool?>(cacheKey);

        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        // Check database
        var blockedIp = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (blockedIp != null && blockedIp.BlockedUntil > DateTime.UtcNow)
        {
            // Cache the blocked status
            var remainingTime = blockedIp.BlockedUntil - DateTime.UtcNow;
            await _cache.SetAsync(cacheKey, true, remainingTime);
            return true;
        }

        // Cache that IP is not blocked (short duration to allow quick unblocking)
        await _cache.SetAsync(cacheKey, false, TimeSpan.FromMinutes(1));
        return false;
    }

    public async Task BlockIpAsync(string ipAddress, TimeSpan duration, string reason)
    {
        var blockedUntil = DateTime.UtcNow.Add(duration);

        // Check if IP is already blocked
        var existingBlock = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (existingBlock != null)
        {
            // Update existing block
            existingBlock.BlockedUntil = blockedUntil;
            existingBlock.Reason = reason;
            existingBlock.UpdatedAt = DateTime.UtcNow;

            _blockedIpRepository.Update(existingBlock);
        }
        else
        {
            // Create new block
            var blockedIp = new BlockedIp
            {
                IpAddress = ipAddress,
                BlockedUntil = blockedUntil,
                Reason = reason,
                FailedAttemptCount = await GetFailedAttemptsAsync(ipAddress)
            };

            await _blockedIpRepository.AddAsync(blockedIp);
        }

        await _unitOfWork.CompleteAsync();

        // Update cache
        var cacheKey = $"blocked_ip:{ipAddress}";
        await _cache.SetAsync(cacheKey, true, duration);

        // Clear failed attempts after blocking
        await ClearFailedAttemptsAsync(ipAddress);

        _logger.LogWarning("IP {IpAddress} has been blocked until {BlockedUntil}. Reason: {Reason}",
            ipAddress, blockedUntil, reason);
    }

    public async Task UnblockIpAsync(string ipAddress)
    {
        var blockedIp = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (blockedIp != null)
        {
            blockedIp.BlockedUntil = DateTime.UtcNow;
            blockedIp.UpdatedAt = DateTime.UtcNow;
            _blockedIpRepository.Update(blockedIp);
            await _unitOfWork.CompleteAsync();
        }

        // Clear cache
        var cacheKey = $"blocked_ip:{ipAddress}";
        await _cache.RemoveAsync(cacheKey);

        // Clear failed attempts
        await ClearFailedAttemptsAsync(ipAddress);

        _logger.LogInformation("IP {IpAddress} has been unblocked", ipAddress);
    }

    public async Task<int> GetFailedAttemptsAsync(string ipAddress)
    {
        var cacheKey = $"failed_attempts:{ipAddress}";
        var attempts = await _cache.GetAsync<int?>(cacheKey);
        return attempts ?? 0;
    }

    public async Task RecordFailedAttemptAsync(string ipAddress, string? email = null)
    {
        var cacheKey = $"failed_attempts:{ipAddress}";
        var attempts = await GetFailedAttemptsAsync(ipAddress);
        attempts++;

        // Store with expiration window
        await _cache.SetAsync(cacheKey, attempts,
            TimeSpan.FromHours(_settings.IpBlockingWindowHours));

        _logger.LogWarning("Failed login attempt {Count} from IP: {IpAddress}, Email: {Email}",
            attempts, ipAddress, email ?? "N/A");

        // Check if we should block the IP
        if (attempts >= _settings.IpBlockingThreshold)
        {
            var blockDuration = CalculateBlockDuration(ipAddress, attempts);
            await BlockIpAsync(ipAddress, blockDuration,
                $"Exceeded maximum failed login attempts ({attempts} attempts)");
        }
    }

    public async Task ClearFailedAttemptsAsync(string ipAddress)
    {
        var cacheKey = $"failed_attempts:{ipAddress}";
        await _cache.RemoveAsync(cacheKey);

        _logger.LogDebug("Cleared failed attempts for IP: {IpAddress}", ipAddress);
    }

    public async Task<IpBlockInfo?> GetBlockInfoAsync(string ipAddress)
    {
        var blockedIp = await _blockedIpRepository.GetByIpAddressAsync(ipAddress);

        if (blockedIp == null)
        {
            return null;
        }

        var failedAttempts = await GetFailedAttemptsAsync(ipAddress);

        return new IpBlockInfo
        {
            IpAddress = ipAddress,
            BlockedAt = blockedIp.CreatedAt,
            BlockedUntil = blockedIp.BlockedUntil ?? DateTime.MaxValue,
            Reason = blockedIp.Reason,
            FailedAttempts = failedAttempts
        };
    }

    /// <summary>
    /// Calculate progressive block duration based on previous blocks
    /// </summary>
    private TimeSpan CalculateBlockDuration(string ipAddress, int attempts)
    {
        // Progressive blocking: duration increases with more attempts
        if (attempts >= _settings.ProgressiveBlockingThreshold)
        {
            return TimeSpan.FromDays(7); // 7 days for severe offenders
        }
        else if (attempts >= _settings.IpBlockingThreshold * 2)
        {
            return TimeSpan.FromHours(72); // 3 days
        }
        else if (attempts >= _settings.IpBlockingThreshold * 1.5)
        {
            return TimeSpan.FromHours(48); // 2 days
        }
        else
        {
            return TimeSpan.FromHours(_settings.IpBlockDurationHours); // Default: 24 hours
        }
    }
}
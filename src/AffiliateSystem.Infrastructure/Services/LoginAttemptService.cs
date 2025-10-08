using AutoMapper;
using Microsoft.Extensions.Logging;
using AffiliateSystem.Application.DTOs.User;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using System.Linq;

namespace AffiliateSystem.Infrastructure.Services;

/// <summary>
/// Service implementation for tracking login attempts
/// </summary>
public class LoginAttemptService : ILoginAttemptService
{
    private readonly ILoginAttemptRepository _repository;
    private readonly IIpBlockingService _ipBlockingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginAttemptService> _logger;

    public LoginAttemptService(
        ILoginAttemptRepository repository,
        IIpBlockingService ipBlockingService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LoginAttemptService> logger)
    {
        _repository = repository;
        _ipBlockingService = ipBlockingService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task RecordAttemptAsync(LoginAttemptDto attempt)
    {
        // Create entity from DTO
        var entity = new LoginAttempt
        {
            Id = Guid.NewGuid(),
            UserId = attempt.UserId,
            IpAddress = attempt.IpAddress,
            IsSuccessful = attempt.IsSuccessful,
            UserAgent = attempt.UserAgent,
            FailureReason = attempt.FailureReason,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        // Handle IP blocking logic
        if (!attempt.IsSuccessful)
        {
            await _ipBlockingService.RecordFailedAttemptAsync(
                attempt.IpAddress,
                attempt.Email);
        }
        else
        {
            // Clear failed attempts on successful login
            await _ipBlockingService.ClearFailedAttemptsAsync(attempt.IpAddress);
        }

        _logger.LogInformation(
            "Login attempt recorded - IP: {IpAddress}, Success: {IsSuccessful}, Reason: {Reason}",
            attempt.IpAddress, attempt.IsSuccessful, attempt.FailureReason ?? "Success");
    }

    public async Task<IEnumerable<LoginAttemptDto>> GetRecentAttemptsAsync(Guid userId, int count = 10)
    {
        var attempts = await _repository.FindAsync(a => a.UserId == userId);

        var recentAttempts = attempts
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .Select(a => new LoginAttemptDto
            {
                UserId = a.UserId,
                IpAddress = a.IpAddress,
                IsSuccessful = a.IsSuccessful,
                AttemptedAt = a.CreatedAt,
                UserAgent = a.UserAgent,
                FailureReason = a.FailureReason
            });

        return recentAttempts;
    }

    public async Task<int> GetFailedAttemptsCountAsync(Guid userId, int hoursWindow = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hoursWindow);
        var attempts = await _repository.FindAsync(a =>
            a.UserId == userId &&
            !a.IsSuccessful &&
            a.CreatedAt >= since);

        return attempts.Count();
    }

    public async Task<IEnumerable<LoginAttemptDto>> GetAttemptsByIpAsync(string ipAddress, int count = 10)
    {
        var attempts = await _repository.FindAsync(a => a.IpAddress == ipAddress);

        var recentAttempts = attempts
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .Select(a => new LoginAttemptDto
            {
                UserId = a.UserId,
                IpAddress = a.IpAddress,
                IsSuccessful = a.IsSuccessful,
                AttemptedAt = a.CreatedAt,
                UserAgent = a.UserAgent,
                FailureReason = a.FailureReason
            });

        return recentAttempts;
    }

    public async Task<LoginAttemptStats> GetStatsAsync(int hoursWindow = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hoursWindow);
        var attempts = await _repository.FindAsync(a => a.CreatedAt >= since);

        var attemptsList = attempts.ToList();

        var stats = new LoginAttemptStats
        {
            TotalAttempts = attemptsList.Count,
            SuccessfulAttempts = attemptsList.Count(a => a.IsSuccessful),
            FailedAttempts = attemptsList.Count(a => !a.IsSuccessful)
        };

        // Get top failed IPs
        stats.TopFailedIps = attemptsList
            .Where(a => !a.IsSuccessful)
            .GroupBy(a => a.IpAddress)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        // Get failure reasons breakdown
        stats.FailureReasons = attemptsList
            .Where(a => !a.IsSuccessful && !string.IsNullOrEmpty(a.FailureReason))
            .GroupBy(a => a.FailureReason!)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }
}
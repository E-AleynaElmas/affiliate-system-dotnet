using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Enums;
using AffiliateSystem.Application.DTOs.Auth;

namespace AffiliateSystem.Tests.Helpers;

/// <summary>
/// Builder class for creating test data
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a test user with default values
    /// </summary>
    public static User CreateUser(
        string email = "test@example.com",
        UserRole role = UserRole.Customer,
        bool isActive = true)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword123",
            PasswordSalt = "salt123",
            FirstName = "John",
            LastName = "Doe",
            Role = role,
            IsActive = isActive,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a manager user with referral code
    /// </summary>
    public static User CreateManager(string email = "manager@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword123",
            PasswordSalt = "salt123",
            FirstName = "Manager",
            LastName = "User",
            Role = UserRole.Manager,
            ReferralCode = "MGR12345",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an admin user
    /// </summary>
    public static User CreateAdmin(string email = "admin@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashedPassword123",
            PasswordSalt = "salt123",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a login request
    /// </summary>
    public static LoginRequest CreateLoginRequest(
        string email = "test@example.com",
        string password = "Password123!",
        string? captchaToken = null)
    {
        return new LoginRequest
        {
            Email = email,
            Password = password,
            CaptchaToken = captchaToken,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0 Test Browser"
        };
    }

    /// <summary>
    /// Creates a register request
    /// </summary>
    public static RegisterRequest CreateRegisterRequest(
        string email = "newuser@example.com",
        string password = "Password123!",
        string? referralCode = null)
    {
        return new RegisterRequest
        {
            Email = email,
            Password = password,
            PasswordConfirm = password,
            FirstName = "New",
            LastName = "User",
            ReferralCode = referralCode,
            CaptchaToken = "test-captcha-token"
        };
    }

    /// <summary>
    /// Creates a referral link
    /// </summary>
    public static ReferralLink CreateReferralLink(
        Guid? createdByUserId = null,
        string code = "REF12345",
        bool isActive = true)
    {
        return new ReferralLink
        {
            Id = Guid.NewGuid(),
            Code = code,
            CreatedByUserId = createdByUserId ?? Guid.NewGuid(),
            IsActive = isActive,
            UsageCount = 0,
            MaxUsages = 10,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a blocked IP
    /// </summary>
    public static BlockedIp CreateBlockedIp(
        string ipAddress = "192.168.1.100",
        bool isPermanent = false)
    {
        return new BlockedIp
        {
            Id = Guid.NewGuid(),
            IpAddress = ipAddress,
            Reason = "Too many failed login attempts",
            BlockedUntil = isPermanent ? null : DateTime.UtcNow.AddHours(24),
            FailedAttemptCount = 10,
            IsManualBlock = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a login attempt
    /// </summary>
    public static LoginAttempt CreateLoginAttempt(
        string email = "test@example.com",
        string ipAddress = "192.168.1.1",
        bool isSuccessful = false)
    {
        return new LoginAttempt
        {
            Id = Guid.NewGuid(),
            Email = email,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
            UserAgent = "Mozilla/5.0 Test Browser",
            FailureReason = isSuccessful ? null : "Invalid password",
            CreatedAt = DateTime.UtcNow
        };
    }
}
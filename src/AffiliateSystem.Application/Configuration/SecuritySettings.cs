namespace AffiliateSystem.Application.Configuration;

/// <summary>
/// Security-related configuration settings
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    /// <summary>
    /// Maximum number of failed login attempts before account lockout
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public int AccountLockoutMinutes { get; set; } = 30;

    /// <summary>
    /// Number of failed attempts from an IP before blocking
    /// </summary>
    public int IpBlockingThreshold { get; set; } = 10;

    /// <summary>
    /// Time window in hours for counting failed IP attempts
    /// </summary>
    public int IpBlockingWindowHours { get; set; } = 1;

    /// <summary>
    /// IP block duration in hours
    /// </summary>
    public int IpBlockDurationHours { get; set; } = 24;

    /// <summary>
    /// JWT token expiration in hours
    /// </summary>
    public int TokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Password minimum length
    /// </summary>
    public int PasswordMinLength { get; set; } = 8;

    /// <summary>
    /// Referral link expiration in days
    /// </summary>
    public int ReferralLinkExpirationDays { get; set; } = 30;

    /// <summary>
    /// Default referral link maximum usages
    /// </summary>
    public int DefaultReferralLinkMaxUsages { get; set; } = 10;

    /// <summary>
    /// Number of failed attempts before progressive blocking
    /// </summary>
    public int ProgressiveBlockingThreshold { get; set; } = 15;

    /// <summary>
    /// Clear failed attempts after this many hours
    /// </summary>
    public int FailedAttemptsClearHours { get; set; } = 12;
}
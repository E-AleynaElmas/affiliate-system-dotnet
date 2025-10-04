namespace AffiliateSystem.Domain.Entities;

/// <summary>
/// Entity for tracking login attempts
/// Used for IP-based blocking and rate limiting
/// </summary>
public class LoginAttempt : BaseEntity
{
    /// <summary>
    /// IP address of the login attempt
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Attempted email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Was the login successful?
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// User agent information (browser/device info)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// User ID if login was successful
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Related user (Navigation Property)
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Attempt time (more meaningful than CreatedAt)
    /// </summary>
    public DateTime AttemptedAt => CreatedAt;

    /// <summary>
    /// Error message (for failed login)
    /// </summary>
    public string? FailureReason { get; set; }
}
namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// DTO for login attempt information
/// </summary>
public class LoginAttemptDto
{
    /// <summary>
    /// IP address of the attempt
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Was the attempt successful?
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Date and time of the attempt
    /// </summary>
    public DateTime AttemptedAt { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Failure reason if unsuccessful
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// User ID if known
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Email address used in the attempt (for tracking purposes)
    /// </summary>
    public string? Email { get; set; }
}
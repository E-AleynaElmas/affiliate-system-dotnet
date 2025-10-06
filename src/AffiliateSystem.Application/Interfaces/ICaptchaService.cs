namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// CAPTCHA validation service interface
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// Validate CAPTCHA token
    /// </summary>
    /// <param name="token">CAPTCHA token from client</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateCaptchaAsync(string token, string? ipAddress = null);

    /// <summary>
    /// Generate simple CAPTCHA challenge (for development/testing)
    /// </summary>
    /// <returns>CAPTCHA challenge data</returns>
    CaptchaChallenge GenerateSimpleCaptcha();

    /// <summary>
    /// Validate simple CAPTCHA answer
    /// </summary>
    bool ValidateSimpleCaptcha(string challengeId, string answer);
}

/// <summary>
/// CAPTCHA challenge data
/// </summary>
public class CaptchaChallenge
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; } = string.Empty;
    public string ImageBase64 { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
}
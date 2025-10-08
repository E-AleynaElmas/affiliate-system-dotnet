namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// Request DTO for changing user password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password confirmation
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
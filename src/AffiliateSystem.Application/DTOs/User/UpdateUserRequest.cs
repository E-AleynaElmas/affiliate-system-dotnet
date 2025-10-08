namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// Request DTO for updating user information
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's company name
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// User's address
    /// </summary>
    public string? Address { get; set; }
}
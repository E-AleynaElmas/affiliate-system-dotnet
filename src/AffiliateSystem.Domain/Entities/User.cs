using AffiliateSystem.Domain.Enums;

namespace AffiliateSystem.Domain.Entities;

/// <summary>
/// User entity
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Username (email address)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Salt value for password hashing
    /// Must be unique for each user
    /// </summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// User's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Referral code for Managers
    /// Null for Customers
    /// </summary>
    public string? ReferralCode { get; set; }

    /// <summary>
    /// ID of the user who referred this user
    /// Populated if registered via referral link
    /// </summary>
    public Guid? ReferredById { get; set; }

    /// <summary>
    /// Referring user (Navigation Property)
    /// </summary>
    public virtual User? ReferredBy { get; set; }

    /// <summary>
    /// Users registered through this user's referral (Navigation Property)
    /// </summary>
    public virtual ICollection<User> ReferredUsers { get; set; } = new List<User>();

    /// <summary>
    /// Is email confirmed?
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Is account active?
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Failed login attempt count
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Account lockout end date
    /// </summary>
    public DateTime? LockoutEndDate { get; set; }

    /// <summary>
    /// User's login attempts (Navigation Property)
    /// </summary>
    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();

    public User()
    {
        IsActive = true;
        EmailConfirmed = false;
        FailedLoginAttempts = 0;
    }
}
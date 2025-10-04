namespace AffiliateSystem.Domain.Enums;

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Customer - Users who register normally
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Manager - Users who register via referral link
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Admin - System administrator
    /// </summary>
    Admin = 3
}
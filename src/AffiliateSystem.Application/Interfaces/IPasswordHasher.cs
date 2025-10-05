namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// Password hashing service interface
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password with salt
    /// </summary>
    (string hash, string salt) HashPassword(string password);

    /// <summary>
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string hash, string salt);
}
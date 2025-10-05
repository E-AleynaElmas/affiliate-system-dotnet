using System.Security.Cryptography;
using System.Text;
using AffiliateSystem.Application.Interfaces;

namespace AffiliateSystem.Application.Services;

/// <summary>
/// Password hashing service implementation using SHA256
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hash password with salt
    /// </summary>
    public (string hash, string salt) HashPassword(string password)
    {
        // Generate salt
        byte[] saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        string salt = Convert.ToBase64String(saltBytes);

        // Hash password with salt
        string hash = ComputeHash(password, salt);

        return (hash, salt);
    }

    /// <summary>
    /// Verify password
    /// </summary>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        string computedHash = ComputeHash(password, salt);
        return computedHash == hash;
    }

    /// <summary>
    /// Compute SHA256 hash
    /// </summary>
    private string ComputeHash(string password, string salt)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            string saltedPassword = password + salt;
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(bytes);
        }
    }
}
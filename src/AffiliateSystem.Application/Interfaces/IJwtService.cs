using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Application.Interfaces;

/// <summary>
/// JWT token generation service interface
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate JWT token for authenticated user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Guid? GetUserIdFromToken(string token);
}
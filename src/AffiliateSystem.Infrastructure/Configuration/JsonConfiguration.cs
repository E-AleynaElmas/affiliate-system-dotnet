using System.Text.Json;
using System.Text.Json.Serialization;

namespace AffiliateSystem.Infrastructure.Configuration;

/// <summary>
/// Centralized JSON serialization configuration
/// </summary>
public static class JsonConfiguration
{
    /// <summary>
    /// Default JSON serialization options used across the application
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// JSON options optimized for logging (compact format)
    /// </summary>
    public static readonly JsonSerializerOptions LoggingOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// JSON options for error responses
    /// </summary>
    public static readonly JsonSerializerOptions ErrorOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}

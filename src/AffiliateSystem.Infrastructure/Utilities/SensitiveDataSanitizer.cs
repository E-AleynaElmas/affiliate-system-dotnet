using System.Text.RegularExpressions;

namespace AffiliateSystem.Infrastructure.Utilities;

/// <summary>
/// Utility class for sanitizing sensitive data in logs and audit trails
/// </summary>
public static class SensitiveDataSanitizer
{
    private static readonly string[] SensitiveFields =
    {
        "password",
        "passwordConfirm",
        "currentPassword",
        "newPassword",
        "token",
        "captchaToken",
        "secret",
        "key",
        "authorization",
        "apiKey"
    };

    /// <summary>
    /// Sanitize JSON string by replacing sensitive field values with [REDACTED]
    /// </summary>
    public static string SanitizeJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        foreach (var field in SensitiveFields)
        {
            json = Regex.Replace(
                json,
                $@"""{field}""\s*:\s*""[^""]+""",
                $@"""{field}"":""[REDACTED]""",
                RegexOptions.IgnoreCase);
        }

        return json;
    }

    /// <summary>
    /// Check if a key name represents sensitive data
    /// </summary>
    public static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        return SensitiveFields.Any(field =>
            key.Contains(field, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sanitize dictionary by replacing sensitive values with [REDACTED]
    /// </summary>
    public static Dictionary<string, object?> SanitizeDictionary(Dictionary<string, object?> data)
    {
        var sanitized = new Dictionary<string, object?>();

        foreach (var kvp in data)
        {
            sanitized[kvp.Key] = IsSensitiveKey(kvp.Key) ? "[REDACTED]" : kvp.Value;
        }

        return sanitized;
    }
}

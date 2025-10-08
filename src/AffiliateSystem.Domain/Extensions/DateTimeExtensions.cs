namespace AffiliateSystem.Domain.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Get DateTime for specified hours ago from now (UTC)
    /// </summary>
    public static DateTime HoursAgo(int hours)
    {
        return DateTime.UtcNow.AddHours(-hours);
    }

    /// <summary>
    /// Get DateTime for specified days ago from now (UTC)
    /// </summary>
    public static DateTime DaysAgo(int days)
    {
        return DateTime.UtcNow.AddDays(-days);
    }

    /// <summary>
    /// Get DateTime for specified minutes ago from now (UTC)
    /// </summary>
    public static DateTime MinutesAgo(int minutes)
    {
        return DateTime.UtcNow.AddMinutes(-minutes);
    }

    /// <summary>
    /// Get DateTime for specified seconds ago from now (UTC)
    /// </summary>
    public static DateTime SecondsAgo(int seconds)
    {
        return DateTime.UtcNow.AddSeconds(-seconds);
    }
}

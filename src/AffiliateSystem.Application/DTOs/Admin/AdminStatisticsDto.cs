namespace AffiliateSystem.Application.DTOs.Admin;

/// <summary>
/// System statistics for admin dashboard
/// </summary>
public class AdminStatisticsDto
{
    /// <summary>
    /// Total number of users in the system
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Number of active users
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Number of blocked users
    /// </summary>
    public int BlockedUsers { get; set; }

    /// <summary>
    /// Total number of managers
    /// </summary>
    public int TotalManagers { get; set; }

    /// <summary>
    /// Total number of customers
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Total number of referral links
    /// </summary>
    public int TotalReferralLinks { get; set; }

    /// <summary>
    /// Number of active referral links
    /// </summary>
    public int ActiveReferralLinks { get; set; }

    /// <summary>
    /// Total login attempts
    /// </summary>
    public int TotalLoginAttempts { get; set; }

    /// <summary>
    /// Failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Number of blocked IPs
    /// </summary>
    public int BlockedIpCount { get; set; }

    /// <summary>
    /// Users registered today
    /// </summary>
    public int UsersRegisteredToday { get; set; }

    /// <summary>
    /// Users registered this week
    /// </summary>
    public int UsersRegisteredThisWeek { get; set; }

    /// <summary>
    /// Users registered this month
    /// </summary>
    public int UsersRegisteredThisMonth { get; set; }
}

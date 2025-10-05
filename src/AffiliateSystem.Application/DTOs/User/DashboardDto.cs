namespace AffiliateSystem.Application.DTOs.User;

/// <summary>
/// Dashboard information DTO
/// </summary>
public class DashboardDto
{
    public UserDto UserInfo { get; set; } = new UserDto();
    public DashboardStats? Stats { get; set; }
}

/// <summary>
/// Dashboard statistics (for Admin users)
/// </summary>
public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalManagers { get; set; }
    public int TotalAdmins { get; set; }
    public int ActiveUsers { get; set; }
    public int BlockedIPs { get; set; }
    public int TodayLoginAttempts { get; set; }
    public int FailedLoginAttempts { get; set; }
}
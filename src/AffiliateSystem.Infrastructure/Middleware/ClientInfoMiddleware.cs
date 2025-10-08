using Microsoft.AspNetCore.Http;

namespace AffiliateSystem.Infrastructure.Middleware;

/// <summary>
/// Middleware to extract and store client information (IP address, user agent) in HttpContext.Items
/// </summary>
public class ClientInfoMiddleware
{
    private readonly RequestDelegate _next;

    public ClientInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract client IP address
        var ipAddress = GetClientIpAddress(context);
        context.Items["ClientIpAddress"] = ipAddress;

        // Extract user agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        context.Items["UserAgent"] = userAgent;

        // Store in HttpContext features for easy access
        context.Features.Set(new ClientInfo
        {
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, get the first one
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header (some proxies use this)
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

/// <summary>
/// Client information extracted from the request
/// </summary>
public class ClientInfo
{
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// Extension methods for accessing client information
/// </summary>
public static class ClientInfoExtensions
{
    /// <summary>
    /// Get client information from HttpContext
    /// </summary>
    public static ClientInfo GetClientInfo(this HttpContext context)
    {
        return context.Features.Get<ClientInfo>() ?? new ClientInfo
        {
            IpAddress = "Unknown",
            UserAgent = "Unknown"
        };
    }

    /// <summary>
    /// Get client IP address from HttpContext
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        return context.GetClientInfo().IpAddress;
    }

    /// <summary>
    /// Get user agent from HttpContext
    /// </summary>
    public static string GetUserAgent(this HttpContext context)
    {
        return context.GetClientInfo().UserAgent;
    }
}
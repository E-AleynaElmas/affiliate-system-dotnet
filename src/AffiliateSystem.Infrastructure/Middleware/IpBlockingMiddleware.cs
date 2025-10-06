using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using System.Net;

namespace AffiliateSystem.Infrastructure.Middleware;

/// <summary>
/// Middleware to block requests from blocked IP addresses
/// </summary>
public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlockingMiddleware> _logger;

    public IpBlockingMiddleware(RequestDelegate next, ILogger<IpBlockingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRepository<BlockedIp> blockedIpRepository)
    {
        var clientIp = GetClientIpAddress(context);

        if (!string.IsNullOrEmpty(clientIp))
        {
            // Check if IP is blocked
            var blockedIp = await blockedIpRepository.SingleOrDefaultAsync(b => b.IpAddress == clientIp);

            if (blockedIp != null && blockedIp.IsActive)
            {
                _logger.LogWarning("Blocked IP {IpAddress} attempted to access {Path}",
                    clientIp, context.Request.Path);

                // Return 403 Forbidden
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Access denied",
                    message = "Your IP address has been blocked due to suspicious activity.",
                    blockedUntil = blockedIp.BlockedUntil?.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Extract client IP address from request
    /// </summary>
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
        return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}
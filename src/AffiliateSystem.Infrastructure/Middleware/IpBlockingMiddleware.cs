using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AffiliateSystem.Application.Interfaces;
using AffiliateSystem.Infrastructure.Configuration;

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

    public async Task InvokeAsync(HttpContext context, IIpBlockingService ipBlockingService)
    {
        var clientIp = context.GetClientIpAddress(); // Use extension method from ClientInfoMiddleware

        if (!string.IsNullOrEmpty(clientIp))
        {
            // Check if IP is blocked using the service
            if (await ipBlockingService.IsBlockedAsync(clientIp))
            {
                _logger.LogWarning("Blocked IP {IpAddress} attempted to access {Path}",
                    clientIp, context.Request.Path);

                // Get detailed block information
                var blockInfo = await ipBlockingService.GetBlockInfoAsync(clientIp);

                // Return 403 Forbidden
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Access denied",
                    message = "Your IP address has been temporarily blocked due to suspicious activity.",
                    code = "IP_BLOCKED",
                    blockedUntil = blockInfo?.BlockedUntil.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    reason = blockInfo?.Reason,
                    remainingTime = blockInfo != null ?
                        (blockInfo.BlockedUntil - DateTime.UtcNow).TotalMinutes : 0
                };

                var jsonResponse = JsonSerializer.Serialize(response, JsonConfiguration.ErrorOptions);

                await context.Response.WriteAsync(jsonResponse);
                return;
            }
        }

        await _next(context);
    }
}
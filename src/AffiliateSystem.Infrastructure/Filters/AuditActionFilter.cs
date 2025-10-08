using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;
using AffiliateSystem.Infrastructure.Middleware;
using AffiliateSystem.Infrastructure.Utilities;

namespace AffiliateSystem.Infrastructure.Filters;

/// <summary>
/// Action filter for auditing sensitive operations
/// </summary>
public class AuditActionFilter : IAsyncActionFilter
{
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(ILogger<AuditActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userEmail = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        var ipAddress = context.HttpContext.GetClientIpAddress();
        var actionName = context.ActionDescriptor.DisplayName;
        var timestamp = DateTime.UtcNow;

        // Create audit entry
        var auditEntry = new
        {
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            Action = actionName,
            Timestamp = timestamp,
            RequestData = GetSanitizedRequestData(context),
            UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
        };

        _logger.LogInformation("AUDIT: User {UserEmail} ({UserId}) performed action {Action} from IP {IpAddress} at {Timestamp}",
            userEmail, userId, actionName, ipAddress, timestamp);

        // Execute the action
        var result = await next();

        // Log the result
        if (result.Exception != null)
        {
            _logger.LogWarning("AUDIT: Action {Action} by user {UserEmail} failed: {Error}",
                actionName, userEmail, result.Exception.Message);
        }
        else
        {
            _logger.LogInformation("AUDIT: Action {Action} by user {UserEmail} completed successfully",
                actionName, userEmail);
        }
    }

    private object? GetSanitizedRequestData(ActionExecutingContext context)
    {
        if (!context.ActionArguments.Any())
            return null;

        var sanitized = new Dictionary<string, object?>();

        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value == null)
            {
                sanitized[argument.Key] = null;
                continue;
            }

            sanitized[argument.Key] = SensitiveDataSanitizer.IsSensitiveKey(argument.Key)
                ? "[REDACTED]"
                : argument.Value;
        }

        return sanitized;
    }
}

/// <summary>
/// Attribute for auditing sensitive operations
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuditAttribute : TypeFilterAttribute
{
    public AuditAttribute() : base(typeof(AuditActionFilter))
    {
    }
}
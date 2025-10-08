using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using AffiliateSystem.Infrastructure.Utilities;
using AffiliateSystem.Infrastructure.Configuration;

namespace AffiliateSystem.Infrastructure.Filters;

/// <summary>
/// Action filter for automatic logging of controller actions
/// </summary>
public class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        var actionName = context.ActionDescriptor.DisplayName;
        var controllerName = context.Controller.GetType().Name;
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        _logger.LogInformation(
            "Executing action {ActionName} on controller {ControllerName} [{HttpMethod} {Path}]",
            actionName, controllerName, httpMethod, path);

        // Log action arguments in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            if (context.ActionArguments.Count > 0)
            {
                var arguments = JsonSerializer.Serialize(context.ActionArguments, JsonConfiguration.LoggingOptions);

                // Don't log sensitive data
                var sanitizedArguments = SensitiveDataSanitizer.SanitizeJson(arguments);
                _logger.LogDebug("Action arguments: {Arguments}", sanitizedArguments);
            }
        }

        var executedContext = await next();

        stopwatch.Stop();

        if (executedContext.Exception != null)
        {
            _logger.LogError(executedContext.Exception,
                "Action {ActionName} on controller {ControllerName} threw exception after {ElapsedMilliseconds}ms",
                actionName, controllerName, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "Executed action {ActionName} on controller {ControllerName} in {ElapsedMilliseconds}ms",
                actionName, controllerName, stopwatch.ElapsedMilliseconds);

            var statusCode = context.HttpContext.Response.StatusCode;
            if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Action {ActionName} returned status code {StatusCode}",
                    actionName, statusCode);
            }
        }
    }
}

/// <summary>
/// Attribute for applying logging to specific actions
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LogActionAttribute : TypeFilterAttribute
{
    public LogActionAttribute() : base(typeof(LoggingActionFilter))
    {
    }
}
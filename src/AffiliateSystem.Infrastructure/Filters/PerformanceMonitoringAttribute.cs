using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AffiliateSystem.Infrastructure.Filters;

/// <summary>
/// AOP filter for monitoring endpoint performance
/// Logs warning when endpoint execution time exceeds threshold
/// </summary>
public class PerformanceMonitoringAttribute : ActionFilterAttribute
{
    private readonly int _thresholdMs;
    private const string StopwatchKey = "PerformanceStopwatch";

    public PerformanceMonitoringAttribute(int thresholdMs = 1000)
    {
        _thresholdMs = thresholdMs;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items[StopwatchKey] = stopwatch;
        base.OnActionExecuting(context);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items[StopwatchKey] is Stopwatch stopwatch)
        {
            stopwatch.Stop();

            var logger = context.HttpContext.RequestServices
                .GetService(typeof(ILogger<PerformanceMonitoringAttribute>)) as ILogger<PerformanceMonitoringAttribute>;

            var actionName = context.ActionDescriptor.DisplayName ?? "Unknown Action";
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > _thresholdMs)
            {
                logger?.LogWarning(
                    "SLOW ENDPOINT: {Action} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                    actionName,
                    elapsedMs,
                    _thresholdMs);
            }
            else
            {
                logger?.LogDebug(
                    "Endpoint {Action} completed in {ElapsedMs}ms",
                    actionName,
                    elapsedMs);
            }
        }

        base.OnActionExecuted(context);
    }
}

/// <summary>
/// Convenience attributes with predefined thresholds
/// </summary>
public class MonitorPerformanceAttribute : PerformanceMonitoringAttribute
{
    public MonitorPerformanceAttribute() : base(1000) { }
}

public class MonitorFastEndpointAttribute : PerformanceMonitoringAttribute
{
    public MonitorFastEndpointAttribute() : base(500) { }
}

public class MonitorSlowEndpointAttribute : PerformanceMonitoringAttribute
{
    public MonitorSlowEndpointAttribute() : base(3000) { }
}

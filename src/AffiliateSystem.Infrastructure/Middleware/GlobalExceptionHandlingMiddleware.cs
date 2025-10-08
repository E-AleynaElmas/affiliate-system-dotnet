using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AffiliateSystem.Application.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace AffiliateSystem.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new BaseResponse<object>
        {
            Success = false,
            Message = GetErrorMessage(exception),
            Data = null
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Unauthorized access";
                break;

            case KeyNotFoundException:
            case FileNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Resource not found";
                break;

            case ArgumentNullException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "A required argument was null";
                break;

            case ArgumentException:
            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Message = "Request timed out";
                break;

            case NotImplementedException:
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                errorResponse.Message = "Feature not implemented";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        // In development, include more details
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            errorResponse.Message = $"{errorResponse.Message}: {exception.Message}";
            errorResponse.Data = new
            {
                Type = exception.GetType().Name,
                StackTrace = exception.StackTrace
            };
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException argNull => $"Required parameter is missing: {argNull.ParamName}",
            ArgumentException argEx => $"Invalid argument: {argEx.Message}",
            InvalidOperationException => "This operation is not allowed in the current state",
            _ => exception.Message
        };
    }
}
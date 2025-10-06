using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AffiliateSystem.Infrastructure.Configuration;

/// <summary>
/// Rate limiting configuration
/// </summary>
public static class RateLimitConfiguration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Load rate limit configuration from appsettings.json
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        // Load IP rate limit policies from appsettings.json
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

        // Register stores
        services.AddMemoryCache();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        // Register the middleware
        services.AddInMemoryRateLimiting();

        return services;
    }

    /// <summary>
    /// Get default rate limit options
    /// </summary>
    public static IpRateLimitOptions GetDefaultOptions()
    {
        return new IpRateLimitOptions
        {
            GeneralRules = new List<RateLimitRule>
            {
                // General API rate limit
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1m",
                    Limit = 60  // 60 requests per minute
                },
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1h",
                    Limit = 1000  // 1000 requests per hour
                },

                // Login endpoint specific limits
                new RateLimitRule
                {
                    Endpoint = "POST:/api/auth/login",
                    Period = "5m",
                    Limit = 5  // 5 login attempts per 5 minutes
                },
                new RateLimitRule
                {
                    Endpoint = "POST:/api/auth/login",
                    Period = "1h",
                    Limit = 20  // 20 login attempts per hour
                },

                // Registration endpoint limits
                new RateLimitRule
                {
                    Endpoint = "POST:/api/auth/register",
                    Period = "1h",
                    Limit = 3  // 3 registrations per hour per IP
                },
                new RateLimitRule
                {
                    Endpoint = "POST:/api/auth/register",
                    Period = "1d",
                    Limit = 10  // 10 registrations per day per IP
                },

                // Referral validation endpoint
                new RateLimitRule
                {
                    Endpoint = "GET:/api/auth/validate-referral/*",
                    Period = "1m",
                    Limit = 10  // 10 referral checks per minute
                },

                // Password change endpoint
                new RateLimitRule
                {
                    Endpoint = "POST:/api/user/change-password",
                    Period = "1h",
                    Limit = 3  // 3 password change attempts per hour
                }
            },
            EnableEndpointRateLimiting = true,
            StackBlockedRequests = false,
            HttpStatusCode = 429,  // Too Many Requests
            RealIpHeader = "X-Real-IP",
            ClientIdHeader = "X-ClientId",
            QuotaExceededResponse = new QuotaExceededResponse
            {
                Content = "{{ \"error\": \"Too many requests\", \"message\": \"You have exceeded the rate limit. Please try again later.\", \"retryAfter\": \"{1}\" }}",
                ContentType = "application/json",
                StatusCode = 429
            }
        };
    }
}
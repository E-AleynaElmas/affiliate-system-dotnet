using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AffiliateSystem.Application.Interfaces;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AffiliateSystem.Infrastructure.Services;

/// <summary>
/// CAPTCHA validation service supporting Google reCAPTCHA and simple math CAPTCHA
/// </summary>
public class CaptchaService : ICaptchaService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CaptchaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly bool _isEnabled;
    private readonly bool _useGoogleRecaptcha;
    private readonly string? _secretKey;
    private readonly string? _siteKey;

    public CaptchaService(
        IConfiguration configuration,
        ILogger<CaptchaService> logger,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        _httpClient = httpClientFactory.CreateClient();

        var captchaSection = _configuration.GetSection("CaptchaSettings");
        _isEnabled = captchaSection.GetValue<bool>("Enabled");
        _useGoogleRecaptcha = captchaSection.GetValue<bool>("UseGoogleRecaptcha");
        _secretKey = captchaSection["SecretKey"];
        _siteKey = captchaSection["SiteKey"];
    }

    public async Task<bool> ValidateCaptchaAsync(string token, string? ipAddress = null)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("CAPTCHA validation is disabled");
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("CAPTCHA token is empty");
            return false;
        }

        if (!_useGoogleRecaptcha)
        {
            return ValidateSimpleCaptchaFromToken(token);
        }

        try
        {
            var parameters = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _secretKey ?? ""),
                new KeyValuePair<string, string>("response", token),
                new KeyValuePair<string, string>("remoteip", ipAddress ?? "")
            });

            var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                parameters);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecaptchaResponse>(jsonString);

                if (result != null && result.Success)
                {
                    // reCAPTCHA v3 score validation
                    if (result.Score.HasValue && result.Score < 0.5)
                    {
                        _logger.LogWarning("CAPTCHA validation failed: Low score {Score}", result.Score);
                        return false;
                    }

                    return true;
                }

                _logger.LogWarning("CAPTCHA validation failed: {ErrorCodes}",
                    string.Join(", ", result?.ErrorCodes ?? new List<string>()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CAPTCHA");
        }

        return false;
    }

    public CaptchaChallenge GenerateSimpleCaptcha()
    {
        var random = new Random();
        var num1 = random.Next(1, 10);
        var num2 = random.Next(1, 10);
        var operation = random.Next(0, 3);

        string question;
        int answer;

        switch (operation)
        {
            case 0:
                question = $"{num1} + {num2}";
                answer = num1 + num2;
                break;
            case 1:
                var max = Math.Max(num1, num2);
                var min = Math.Min(num1, num2);
                question = $"{max} - {min}";
                answer = max - min;
                break;
            case 2:
                question = $"{num1} × {num2}";
                answer = num1 * num2;
                break;
            default:
                question = $"{num1} + {num2}";
                answer = num1 + num2;
                break;
        }

        var challenge = new CaptchaChallenge
        {
            Question = $"What is {question}?",
            ImageBase64 = GenerateSimpleImage(question)
        };

        var cacheKey = $"captcha_{challenge.Id}";
        _cache.Set(cacheKey, answer.ToString(), TimeSpan.FromMinutes(5));

        return challenge;
    }

    public bool ValidateSimpleCaptcha(string challengeId, string answer)
    {
        var cacheKey = $"captcha_{challengeId}";

        if (_cache.TryGetValue<string>(cacheKey, out var correctAnswer))
        {
            _cache.Remove(cacheKey);
            return string.Equals(correctAnswer, answer, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private bool ValidateSimpleCaptchaFromToken(string token)
    {
        var parts = token.Split(':');
        if (parts.Length == 2)
        {
            return ValidateSimpleCaptcha(parts[0], parts[1]);
        }

        return false;
    }

    private string GenerateSimpleImage(string text)
    {
        var bytes = Encoding.UTF8.GetBytes($"CAPTCHA: {text}");
        return Convert.ToBase64String(bytes);
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
        public double? Score { get; set; }
        public string? Action { get; set; }
        public DateTime ChallengeTs { get; set; }
        public string? Hostname { get; set; }
        public List<string> ErrorCodes { get; set; } = new List<string>();
    }
}
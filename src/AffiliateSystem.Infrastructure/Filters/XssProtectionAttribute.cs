using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AffiliateSystem.Infrastructure.Filters;

/// <summary>
/// Action filter to prevent XSS attacks by sanitizing input
/// </summary>
public class XssProtectionAttribute : ActionFilterAttribute
{
    private readonly bool _allowHtml;
    private readonly string[] _dangerousTags = new[]
    {
        "script", "iframe", "object", "embed", "form", "input",
        "button", "select", "textarea", "style", "link", "meta", "base"
    };

    private readonly string[] _dangerousAttributes = new[]
    {
        "onclick", "onload", "onerror", "onmouseover", "onmouseout",
        "onkeydown", "onkeypress", "onkeyup", "onsubmit", "onchange",
        "onfocus", "onblur", "javascript:", "data:", "vbscript:"
    };

    public XssProtectionAttribute(bool allowHtml = false)
    {
        _allowHtml = allowHtml;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value != null)
            {
                var sanitized = SanitizeObject(argument.Value);
                context.ActionArguments[argument.Key] = sanitized;
            }
        }

        base.OnActionExecuting(context);
    }

    /// <summary>
    /// Sanitize object recursively
    /// </summary>
    private object SanitizeObject(object obj)
    {
        if (obj == null)
            return obj;

        var type = obj.GetType();

        // Handle string
        if (type == typeof(string))
        {
            return SanitizeString(obj.ToString());
        }

        // Handle collections
        if (obj is IEnumerable<object> collection)
        {
            var sanitizedList = new List<object>();
            foreach (var item in collection)
            {
                sanitizedList.Add(SanitizeObject(item));
            }
            return sanitizedList;
        }

        // Handle complex objects
        if (type.IsClass && type != typeof(string))
        {
            foreach (var property in type.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(obj);
                    if (value != null)
                    {
                        var sanitizedValue = SanitizeObject(value);
                        property.SetValue(obj, sanitizedValue);
                    }
                }
            }
        }

        return obj;
    }

    /// <summary>
    /// Sanitize string to prevent XSS
    /// </summary>
    private string SanitizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // First, decode any HTML entities
        var decoded = HttpUtility.HtmlDecode(input);

        if (!_allowHtml)
        {
            // Strip all HTML tags if HTML is not allowed
            return HttpUtility.HtmlEncode(decoded);
        }

        // Remove dangerous tags
        foreach (var tag in _dangerousTags)
        {
            var pattern = $@"<\s*{tag}[^>]*>.*?<\s*/\s*{tag}\s*>|<\s*{tag}[^>]*\s*/?\s*>";
            decoded = Regex.Replace(decoded, pattern, string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        // Remove dangerous attributes
        foreach (var attr in _dangerousAttributes)
        {
            var pattern = $@"{attr}\s*=\s*['""]?[^'"">\s]*['""]?";
            decoded = Regex.Replace(decoded, pattern, string.Empty,
                RegexOptions.IgnoreCase);
        }

        // Remove javascript: and data: protocols
        decoded = Regex.Replace(decoded, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
        decoded = Regex.Replace(decoded, @"data\s*:", string.Empty, RegexOptions.IgnoreCase);
        decoded = Regex.Replace(decoded, @"vbscript\s*:", string.Empty, RegexOptions.IgnoreCase);

        return decoded;
    }
}

/// <summary>
/// Global XSS protection for all controllers
/// </summary>
public class GlobalXssProtectionAttribute : IActionFilter
{
    private readonly XssProtectionAttribute _xssProtection;

    public GlobalXssProtectionAttribute()
    {
        _xssProtection = new XssProtectionAttribute(false);
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _xssProtection.OnActionExecuting(context);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Add security headers
        if (context.HttpContext.Response != null)
        {
            // Content Security Policy
            context.HttpContext.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");

            // X-Content-Type-Options
            context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // X-Frame-Options
            context.HttpContext.Response.Headers.Add("X-Frame-Options", "DENY");

            // X-XSS-Protection
            context.HttpContext.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Referrer Policy
            context.HttpContext.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        }
    }
}
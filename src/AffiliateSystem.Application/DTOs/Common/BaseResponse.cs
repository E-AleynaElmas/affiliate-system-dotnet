namespace AffiliateSystem.Application.DTOs.Common;

/// <summary>
/// Base response wrapper for API responses
/// </summary>
public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Create success response
    /// </summary>
    public static BaseResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Create error response
    /// </summary>
    public static BaseResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
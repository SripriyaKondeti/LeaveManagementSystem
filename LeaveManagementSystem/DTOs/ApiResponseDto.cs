using System.Text.Json.Serialization;

namespace LeaveManagementSystem.DTOs;

/// <summary>
/// Standard response envelope returned by the LMS API.
/// </summary>
public class ApiResponseDto<T>
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    public static ApiResponseDto<T> Success(
        int statusCode,
        string message,
        T? data) => new()
        {
            StatusCode = statusCode,
            Message = message,
            Data = data
        };

    public static ApiResponseDto<T> Failure(
        int statusCode,
        string message,
        string? traceId = null,
        IDictionary<string, string[]>? errors = null) => new()
        {
            StatusCode = statusCode,
            Message = message,
            TraceId = traceId,
            Errors = errors
        };
}

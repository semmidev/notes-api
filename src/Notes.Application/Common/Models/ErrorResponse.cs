namespace Notes.Application.Common.Models;

public record FieldError(string Field, string Message);

public record ErrorResponse(
    bool Success,
    int StatusCode,
    string ErrorCode,
    string Message,
    IReadOnlyList<FieldError>? Errors,
    DateTimeOffset Timestamp,
    string? TraceId)
{
    public static ErrorResponse Create(
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyList<FieldError>? errors = null,
        string? traceId = null) =>
        new(
            Success: false,
            StatusCode: statusCode,
            ErrorCode: errorCode,
            Message: message,
            Errors: errors,
            Timestamp: DateTimeOffset.UtcNow,
            TraceId: traceId);
}

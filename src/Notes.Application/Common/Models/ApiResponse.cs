namespace Notes.Application.Common.Models;

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    int StatusCode = 200)
{
    public static ApiResponse<T> Ok(T data, string message = "Sukses") =>
        new(true, message, data, 200);

    public static ApiResponse<T> Created(T data, string message = "Berhasil dibuat") =>
        new(true, message, data, 201);

    public static ApiResponse<T> Failure(string message, int statusCode = 400) =>
        new(false, message, default, statusCode);
}

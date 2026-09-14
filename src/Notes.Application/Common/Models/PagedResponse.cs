namespace Notes.Application.Common.Models;

public record PagedResponse<T>(
    bool Success,
    string Message,
    IReadOnlyList<T> Data,
    PaginationMeta Paging,
    int StatusCode = 200)
{
    public static PagedResponse<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        long totalRecords,
        string message = "Sukses mengambil data")
    {
        var paging = PaginationMeta.Create(pageNumber, pageSize, totalRecords);

        return new PagedResponse<T>(
            Success: true,
            Message: message,
            Data: items,
            Paging: paging,
            StatusCode: 200);
    }
}

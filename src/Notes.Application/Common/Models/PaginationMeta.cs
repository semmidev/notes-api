namespace Notes.Application.Common.Models;

public record PaginationMeta(
    int PageNumber,
    int PageSize,
    int TotalPages,
    long TotalRecords)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginationMeta Create(int pageNumber, int pageSize, long totalRecords)
    {
        var totalPages = pageSize > 0
            ? (int)Math.Ceiling(totalRecords / (double)pageSize)
            : 0;

        return new PaginationMeta(
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalPages: totalPages,
            TotalRecords: totalRecords);
    }
}

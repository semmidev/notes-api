using Microsoft.EntityFrameworkCore;
using Notes.Application.Abstractions;
using Notes.Domain.Entities;
using Notes.Infrastructure.Persistence;

using Notes.Application.Common.Models;

namespace Notes.Infrastructure.Repositories;

public sealed class NoteRepository(AppDbContext dbContext) : INoteRepository
{
    public async Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedAsync(
        PaginationParams paginationParams,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Notes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchKeyword))
        {
            var keyword = paginationParams.SearchKeyword.Trim().ToLower();
            query = query.Where(x => EF.Functions.Like(x.Title.ToLower(), $"%{keyword}%") ||
                                     EF.Functions.Like(x.Content.ToLower(), $"%{keyword}%"));
        }

        query = (paginationParams.SortBy?.ToLower(), paginationParams.SortOrder?.ToLower()) switch
        {
            ("title", "asc") => query.OrderBy(x => x.Title),
            ("title", "desc") => query.OrderByDescending(x => x.Title),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Notes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Note note, CancellationToken cancellationToken)
    {
        await dbContext.Notes.AddAsync(note, cancellationToken);
    }

    public void Remove(Note note)
    {
        dbContext.Notes.Remove(note);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Notes.Application.Notes.NoteAnalyticsResponse> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfToday = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var totalNotes = await dbContext.Notes.LongCountAsync(cancellationToken);

        var notesCreatedToday = await dbContext.Notes
            .Where(x => x.CreatedAt >= startOfToday)
            .LongCountAsync(cancellationToken);

        var notesCreatedThisMonth = await dbContext.Notes
            .Where(x => x.CreatedAt >= startOfMonth)
            .LongCountAsync(cancellationToken);

        var avgLength = totalNotes > 0
            ? await dbContext.Notes.AverageAsync(x => (double)x.Content.Length, cancellationToken)
            : 0;

        var monthlyStats = await dbContext.Database
            .SqlQuery<Notes.Application.Notes.MonthlyNoteStat>($"""
                SELECT 
                    TO_CHAR(created_at, 'YYYY-MM') AS "MonthYear",
                    COUNT(*)::bigint AS "Count"
                FROM notes
                GROUP BY TO_CHAR(created_at, 'YYYY-MM')
                ORDER BY "MonthYear" DESC
                LIMIT 12
                """)
            .ToListAsync(cancellationToken);

        return new Notes.Application.Notes.NoteAnalyticsResponse(
            TotalNotes: totalNotes,
            NotesCreatedToday: notesCreatedToday,
            NotesCreatedThisMonth: notesCreatedThisMonth,
            AverageContentLength: Math.Round(avgLength, 2),
            MonthlyStats: monthlyStats);
    }
}

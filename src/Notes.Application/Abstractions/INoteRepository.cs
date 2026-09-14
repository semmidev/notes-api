using Notes.Application.Notes;
using Notes.Domain.Entities;

namespace Notes.Application.Abstractions;

/// <summary>
/// Contract persistence. Application hanya tahu operasi yang dibutuhkan,
/// bukan bagaimana PostgreSQL menjalankan operasi tersebut.
/// </summary>
public interface INoteRepository
{
    Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Note note, CancellationToken cancellationToken);
    void Remove(Note note);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<NoteAnalyticsResponse> GetAnalyticsAsync(CancellationToken cancellationToken);
}

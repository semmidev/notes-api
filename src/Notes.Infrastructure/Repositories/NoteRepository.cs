using Microsoft.EntityFrameworkCore;
using Notes.Application.Abstractions;
using Notes.Domain.Entities;
using Notes.Infrastructure.Persistence;

namespace Notes.Infrastructure.Repositories;

public sealed class NoteRepository(AppDbContext dbContext) : INoteRepository
{
    public async Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Notes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
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
}

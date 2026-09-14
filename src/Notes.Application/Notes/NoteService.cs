using Notes.Application.Abstractions;
using Notes.Domain.Entities;

namespace Notes.Application.Notes;

/// <summary>
/// Use case/application service untuk Notes.
/// Business flow berada di sini, sementara aturan entity tetap berada di Domain.
/// </summary>
public sealed class NoteService(INoteRepository repository)
{
    public async Task<IReadOnlyList<NoteResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var notes = await repository.GetAllAsync(cancellationToken);
        return notes.Select(ToResponse).ToList();
    }

    public async Task<NoteResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken);
        return note is null ? null : ToResponse(note);
    }

    public async Task<NoteResponse> CreateAsync(CreateNoteCommand command, CancellationToken cancellationToken)
    {
        // Entity Domain yang memvalidasi dan membuat state awal note.
        var note = Note.Create(command.Title, command.Content);

        await repository.AddAsync(note, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(note);
    }

    public async Task<NoteResponse?> UpdateAsync(Guid id, UpdateNoteCommand command, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken);
        if (note is null)
            return null;

        // Business rule update tetap dipanggil melalui method Domain.
        note.Update(command.Title, command.Content);
        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(note);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await repository.GetByIdAsync(id, cancellationToken);
        if (note is null)
            return false;

        repository.Remove(note);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static NoteResponse ToResponse(Note note) => new(
        note.Id,
        note.Title,
        note.Content,
        note.CreatedAt,
        note.UpdatedAt);
}

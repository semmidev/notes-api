namespace Notes.Application.Notes;

public sealed record CreateNoteCommand(string Title, string Content);
public sealed record UpdateNoteCommand(string Title, string Content);

public sealed record NoteResponse(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

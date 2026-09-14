namespace Notes.Application.Notes;

public record MonthlyNoteStat(
    string MonthYear,
    long Count);

public record NoteAnalyticsResponse(
    long TotalNotes,
    long NotesCreatedToday,
    long NotesCreatedThisMonth,
    double AverageContentLength,
    IReadOnlyList<MonthlyNoteStat> MonthlyStats);

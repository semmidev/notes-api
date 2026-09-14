using System.ComponentModel.DataAnnotations;

namespace Notes.Api.Contracts;

public sealed record CreateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [Required] string Content);

public sealed record UpdateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [Required] string Content);

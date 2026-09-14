using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes.Api.Contracts;
using Notes.Application.Common.Models;
using Notes.Application.Notes;

namespace Notes.Api.Controllers;

[ApiController]
[Route("api/v1/notes")]
[Authorize]
public sealed class NotesController(NoteService noteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<NoteResponse>>> GetAll(
        [FromQuery] PaginationParams paginationParams,
        CancellationToken cancellationToken)
    {
        var result = await noteService.GetPagedAsync(paginationParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<NoteAnalyticsResponse>>> GetAnalytics(CancellationToken cancellationToken)
    {
        var analytics = await noteService.GetAnalyticsAsync(cancellationToken);
        return Ok(ApiResponse<NoteAnalyticsResponse>.Ok(analytics, "Statistik analitik catatan berhasil diambil"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<NoteResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var note = await noteService.GetByIdAsync(id, cancellationToken);

        return note is null
            ? NotFound(ErrorResponse.Create(
                StatusCodes.Status404NotFound,
                "NOTE_NOT_FOUND",
                "Note tidak ditemukan.",
                traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<NoteResponse>.Ok(note));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NoteResponse>>> Create(
        CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await noteService.CreateAsync(
            new CreateNoteCommand(request.Title, request.Content),
            cancellationToken);

        var response = ApiResponse<NoteResponse>.Created(note);
        return CreatedAtAction(nameof(GetById), new { id = note.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<NoteResponse>>> Update(
        Guid id,
        UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await noteService.UpdateAsync(
            id,
            new UpdateNoteCommand(request.Title, request.Content),
            cancellationToken);

        return note is null
            ? NotFound(ErrorResponse.Create(
                StatusCodes.Status404NotFound,
                "NOTE_NOT_FOUND",
                "Note tidak ditemukan.",
                traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<NoteResponse>.Ok(note, "Note berhasil diperbarui"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await noteService.DeleteAsync(id, cancellationToken);

        return deleted
            ? Ok(ApiResponse<object>.Ok(null!, "Note berhasil dihapus"))
            : NotFound(ErrorResponse.Create(
                StatusCodes.Status404NotFound,
                "NOTE_NOT_FOUND",
                "Note tidak ditemukan.",
                traceId: HttpContext.TraceIdentifier));
    }
}

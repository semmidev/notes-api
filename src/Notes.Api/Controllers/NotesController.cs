using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes.Api.Contracts;
using Notes.Application.Notes;

namespace Notes.Api.Controllers;

[ApiController]
[Route("api/v1/notes")]
[Authorize]
public sealed class NotesController(NoteService noteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoteResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var notes = await noteService.GetAllAsync(cancellationToken);
        return Ok(notes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var note = await noteService.GetByIdAsync(id, cancellationToken);

        return note is null
            ? NotFound(new { message = "Note tidak ditemukan." })
            : Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> Create(
        CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await noteService.CreateAsync(
            new CreateNoteCommand(request.Title, request.Content),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, note);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NoteResponse>> Update(
        Guid id,
        UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await noteService.UpdateAsync(
            id,
            new UpdateNoteCommand(request.Title, request.Content),
            cancellationToken);

        return note is null
            ? NotFound(new { message = "Note tidak ditemukan." })
            : Ok(note);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await noteService.DeleteAsync(id, cancellationToken);

        return deleted
            ? NoContent()
            : NotFound(new { message = "Note tidak ditemukan." });
    }
}

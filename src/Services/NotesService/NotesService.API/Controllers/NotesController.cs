using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesService.Application.DTOs;
using NotesService.Application.Features.Notes.Commands.CreateNote;
using NotesService.Application.Features.Notes.Commands.DeleteNote;
using NotesService.Application.Features.Notes.Commands.UpdateNote;
using NotesService.Application.Features.Notes.Queries.GetAllNotes;
using NotesService.Application.Features.Notes.Queries.GetNotesByIds;

namespace NotesService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] //  default secured
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // CREATE NOTE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto dto)
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Valid userId claim was not found in the token.");

        var id = await _mediator.Send(new CreateNoteCommand(userId, dto));
        return Ok(new { id });
    }

    //  GET ALL NOTES
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Valid userId claim was not found in the token.");

        var notes = await _mediator.Send(new GetAllNotesQuery(userId));
        return Ok(notes);
    }

    //  USER-BASED FETCH (requires JWT)
    [HttpPost("query/by-ids")]
    public async Task<IActionResult> GetByIds([FromBody] GetNotesByIdsDto dto)
    {
        if (!TryGetUserIdFromToken(out var userId))
            return Unauthorized("Valid userId claim was not found in the token.");

        var notes = await _mediator.Send(new GetNotesByIdsQuery(userId, dto.NoteIds));
        return Ok(notes);
    }

    //  INTERNAL SERVICE INVOCATION (NO AUTH)
    [AllowAnonymous]
    [HttpPost("internal/by-ids")]
    public async Task<IActionResult> GetByIdsInternal([FromBody] GetNotesByIdsDto dto)
    {
        //  No user filtering here (trusted internal call)
        var notes = await _mediator.Send(new GetNotesByIdsQuery(0, dto.NoteIds));
        return Ok(notes);
    }

    //  UPDATE NOTE
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateNoteDto dto)
    {
        var result = await _mediator.Send(new UpdateNoteCommand(dto));

        if (!result) return NotFound();
        return Ok("Updated successfully");
    }

    //  DELETE NOTE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteNoteCommand(id));

        if (!result) return NotFound();
        return Ok("Deleted successfully");
    }

    // TOKEN PARSER
    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }
}
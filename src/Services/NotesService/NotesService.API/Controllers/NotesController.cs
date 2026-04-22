using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesService.Application.DTOs;
using NotesService.Application.Features.Notes.Commands.CreateNote;
using NotesService.Application.Features.Notes.Commands.DeleteNote;
using NotesService.Application.Features.Notes.Commands.UpdateNote;
using NotesService.Application.Features.Notes.Queries.GetAllNotes;

namespace NotesService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto dto)
    {
        if (!TryGetUserIdFromToken(out var userId))
        {
            return Unauthorized("Valid userId claim was not found in the token.");
        }

        var id = await _mediator.Send(new CreateNoteCommand(userId, dto));

        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetUserIdFromToken(out var userId))
        {
            return Unauthorized("Valid userId claim was not found in the token.");
        }

        var notes = await _mediator.Send(new GetAllNotesQuery(userId));

        return Ok(notes);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateNoteDto dto)
    {
        var result = await _mediator.Send(new UpdateNoteCommand(dto));

        if (!result) return NotFound();

        return Ok("Updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteNoteCommand(id));

        if (!result) return NotFound();

        return Ok("Deleted successfully");
    }

    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }
}

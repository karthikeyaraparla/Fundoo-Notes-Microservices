using CollaboratorService.Application.DTOs;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Features.Collaborators.Commands.RemoveCollaborator;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollaboratorController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollaboratorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Add collaborator
    [HttpPost]
    public async Task<IActionResult> Add(AddCollaboratorDto dto)
    {
        if (!TryGetUserIdFromToken(out var userId))
        {
            return Unauthorized("Valid userId claim was not found in the token.");
        }

        await _mediator.Send(new AddCollaboratorCommand(userId, dto));

        return Ok("Collaborator added");
    }

    // Remove collaborator
    [HttpDelete]
    public async Task<IActionResult> Remove(int noteId, int collaboratorUserId)
    {
        await _mediator.Send(new RemoveCollaboratorCommand(noteId, collaboratorUserId));

        return Ok("Collaborator removed");
    }

    private bool TryGetUserIdFromToken(out int userId)
    {
        var userIdClaim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }
}

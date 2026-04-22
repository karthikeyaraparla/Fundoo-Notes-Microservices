using CollaboratorService.Application.DTOs;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Features.Collaborators.Commands.RemoveCollaborator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
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
        var userId = int.Parse(User.FindFirst("userId")!.Value);

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
}
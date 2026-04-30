using MediatR;
using Microsoft.AspNetCore.Mvc;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;

namespace CollaboratorService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollaboratorController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollaboratorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Add collaborator to a note
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> AddCollaborator(
        [FromBody] AddCollaboratorCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
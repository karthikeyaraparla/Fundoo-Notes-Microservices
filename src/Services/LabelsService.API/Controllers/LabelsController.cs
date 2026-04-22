using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LabelsService.Application.DTOs;
using LabelsService.Application.Features.Labels.Commands.AssignLabel;
using LabelsService.Application.Features.Labels.Commands.DeleteLabel;
using LabelsService.Application.Features.Labels.Commands.RemoveLabel;

namespace LabelsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabelController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Helper to get UserId from JWT
    private int GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid token");

        return int.Parse(userId);
    }

    // CREATE LABEL
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabelDto dto)
    {
        var labelId = await _mediator.Send(
            new CreateLabelCommand
            {
                Dto = dto,
                UserId = GetUserId()
            }
        );

        return Ok(new
        {
            message = "Label created successfully",
            labelId
        });
    }

    //  DELETE LABEL
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(
            new DeleteLabelCommand(id, GetUserId())
        );

        return Ok(new
        {
            message = result ? "Label deleted" : "Label not found"
        });
    }

    //  ASSIGN LABEL TO NOTE
    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignLabelDto dto)
    {
        var result = await _mediator.Send(
            new AssignLabelCommand(dto.NoteId, dto.LabelId)
        );

        return Ok(new
        {
            message = result ? "Label assigned to note" : "Failed"
        });
    }

    //  REMOVE LABEL FROM NOTE
    [HttpPost("remove")]
    public async Task<IActionResult> Remove([FromBody] AssignLabelDto dto)
    {
        var result = await _mediator.Send(
            new RemoveLabelCommand(dto.NoteId, dto.LabelId)
        );

        return Ok(new
        {
            message = result ? "Label removed from note" : "Failed"
        });
    }

   
}
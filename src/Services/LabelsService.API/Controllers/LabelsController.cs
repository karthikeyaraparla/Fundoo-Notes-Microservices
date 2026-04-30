using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LabelsService.Application.DTOs;
using LabelsService.Application.Features.Labels.Commands.AssignLabel;
using LabelsService.Application.Features.Labels.Commands.DeleteLabel;
using LabelsService.Application.Features.Labels.Commands.RemoveLabel;
using LabelsService.Application.Features.Labels.Queries.GetNotesByLabelName;

namespace LabelsService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LabelController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // 🔑 Extract userId safely
    private int GetUserId()
    {
        var userId = User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var parsedId))
            throw new UnauthorizedAccessException("Invalid or missing userId in token");

        return parsedId;
    }

    // ✅ CREATE LABEL
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabelDto dto)
    {
        var labelId = await _mediator.Send(new CreateLabelCommand
        {
            Dto = dto,
            UserId = GetUserId()
        });

        return Ok(new
        {
            message = "Label created successfully",
            labelId
        });
    }

    // 🗑 DELETE LABEL
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

    // 🔗 ASSIGN LABEL
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

    // ❌ REMOVE LABEL
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

    // 🔥 MAIN FEATURE (SERVICE INVOCATION + CACHE)
    [HttpGet("{labelName}/notes")]
    public async Task<IActionResult> GetNotesByLabelName(string labelName)
    {
        var userId = GetUserId();

        // Optional: pass auth header if needed downstream
        var authHeader = Request.Headers.Authorization.ToString();

        var notes = await _mediator.Send(
            new GetNotesByLabelNameQuery(
                userId,
                labelName,
                authHeader
            )
        );

        return Ok(notes);
    }
}
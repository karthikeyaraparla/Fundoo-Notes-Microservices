using Dapr;
using Microsoft.AspNetCore.Mvc;
using EmailService.API.Models;
using EmailService.API.Services;

namespace EmailService.API.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly ICollaboratorInvitationEmailSender _emailSender;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        ICollaboratorInvitationEmailSender emailSender,
        ILogger<EmailController> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    [Topic("rabbitmq-pubsub", "collaborator-invitations")]
    [HttpPost("collaborator-invitations")]
    public async Task<IActionResult> SendEmail(
        [FromBody] CollaboratorInvitationRequestedEvent data,
        CancellationToken cancellationToken)
    {
        await _emailSender.SendAsync(data, cancellationToken);

        _logger.LogInformation(
            "Email sent → NoteId: {NoteId}, Email: {Email}",
            data.NoteId,
            data.CollaboratorEmail
        );

        return Ok();
    }
}
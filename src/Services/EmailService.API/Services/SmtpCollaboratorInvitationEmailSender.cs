using System.Net;
using System.Net.Mail;
using EmailService.API.Models;
using Microsoft.Extensions.Logging;

namespace EmailService.API.Services;

public class SmtpCollaboratorInvitationEmailSender : ICollaboratorInvitationEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpCollaboratorInvitationEmailSender> _logger;

    public SmtpCollaboratorInvitationEmailSender(
        SmtpSettings settings,
        ILogger<SmtpCollaboratorInvitationEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(
        CollaboratorInvitationRequestedEvent invitationEvent,
        CancellationToken cancellationToken)
    {
        ValidateSettings();

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = $"Fundoo note collaboration invite for note #{invitationEvent.NoteId}",
            Body =
                $"You have been added as a collaborator on note #{invitationEvent.NoteId} " +
                $"by user #{invitationEvent.OwnerUserId}.{Environment.NewLine}" +
                "Open Fundoo Notes to view the shared note.",
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(invitationEvent.CollaboratorEmail));

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = false,
            Timeout = 10000 // 10 seconds
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            smtpClient.Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            );
        }

        try
        {
            using var registration = cancellationToken.Register(smtpClient.SendAsyncCancel);

            await smtpClient.SendMailAsync(message, cancellationToken);

            _logger.LogInformation(
                "Email successfully sent to {Email} for NoteId {NoteId}",
                invitationEvent.CollaboratorEmail,
                invitationEvent.NoteId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Email} for NoteId {NoteId}",
                invitationEvent.CollaboratorEmail,
                invitationEvent.NoteId
            );

            throw;
        }
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("SmtpSettings:Host is required.");

        if (_settings.Port <= 0)
            throw new InvalidOperationException("SmtpSettings:Port must be greater than 0.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            throw new InvalidOperationException("SmtpSettings:FromEmail is required.");
    }
}
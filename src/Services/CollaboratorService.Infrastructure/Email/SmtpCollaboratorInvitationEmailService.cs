using System.Net;
using System.Net.Mail;
using CollaboratorService.Application.Interfaces;

namespace CollaboratorService.Infrastructure.Email;

public class SmtpCollaboratorInvitationEmailService : ICollaboratorInvitationEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpCollaboratorInvitationEmailService(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendCollaboratorInvitationAsync(
        string collaboratorEmail,
        int noteId,
        int ownerUserId,
        CancellationToken cancellationToken)
    {
        ValidateSettings();

        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = $"Fundoo note collaboration invite for note #{noteId}",
            Body =
                $"You have been added as a collaborator on note #{noteId} by user #{ownerUserId}.{Environment.NewLine}" +
                "Open Fundoo Notes to view the shared note.",
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(collaboratorEmail));

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        using var registration = cancellationToken.Register(smtpClient.SendAsyncCancel);
        await smtpClient.SendMailAsync(message, cancellationToken);
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

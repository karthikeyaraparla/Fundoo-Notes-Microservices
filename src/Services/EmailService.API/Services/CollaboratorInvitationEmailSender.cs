using System.Net;
using System.Net.Mail;
using EmailService.API.Models;

namespace EmailService.API.Services;

public class CollaboratorInvitationEmailSender : ICollaboratorInvitationEmailSender
{
    public async Task SendAsync(
        CollaboratorInvitationRequestedEvent data,
        CancellationToken cancellationToken)
    {
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                "YOUR_EMAIL@gmail.com",
                "YOUR_APP_PASSWORD"
            ),
            EnableSsl = true
        };

        var message = new MailMessage(
            "YOUR_EMAIL@gmail.com",
            data.CollaboratorEmail,
            "Collaborator Invitation",
            $"You were added as collaborator to Note {data.NoteId}"
        );

        await smtp.SendMailAsync(message, cancellationToken);
    }
}
using EmailService.API.Models;

namespace EmailService.API.Services;

public interface ICollaboratorInvitationEmailSender
{
    Task SendAsync(CollaboratorInvitationRequestedEvent data, CancellationToken cancellationToken);
}
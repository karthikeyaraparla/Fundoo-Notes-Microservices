namespace CollaboratorService.Application.Interfaces;

public interface ICollaboratorInvitationEmailService
{
    Task SendCollaboratorInvitationAsync(
        string collaboratorEmail,
        int noteId,
        int ownerUserId,
        CancellationToken cancellationToken);
}

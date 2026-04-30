namespace CollaboratorService.Application.Events;

public record CollaboratorInvitationRequestedEvent(
    string CollaboratorEmail,
    int NoteId,
    int OwnerUserId
);

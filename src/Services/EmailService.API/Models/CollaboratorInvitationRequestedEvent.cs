namespace EmailService.API.Models;

public class CollaboratorInvitationRequestedEvent
{
    public string CollaboratorEmail { get; set; } = string.Empty;
    public int NoteId { get; set; }
    public int OwnerUserId { get; set; }
}
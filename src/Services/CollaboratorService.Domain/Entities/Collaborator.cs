namespace CollaboratorService.Domain.Entities;

// Pure domain model
public class Collaborator
{
    public int Id { get; set; }

    public int NoteId { get; set; }

    public int OwnerUserId { get; set; }

    public int CollaboratorUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}

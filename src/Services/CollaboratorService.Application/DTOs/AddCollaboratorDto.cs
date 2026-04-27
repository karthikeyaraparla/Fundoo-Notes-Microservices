namespace CollaboratorService.Application.DTOs;

public record AddCollaboratorDto(int NoteId, int CollaboratorUserId, string CollaboratorEmail);

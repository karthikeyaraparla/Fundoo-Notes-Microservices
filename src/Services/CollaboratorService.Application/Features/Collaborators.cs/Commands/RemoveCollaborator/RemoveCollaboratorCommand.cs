using MediatR;

namespace CollaboratorService.Application.Features.Collaborators.Commands.RemoveCollaborator;

public record RemoveCollaboratorCommand(int NoteId, int CollaboratorUserId) : IRequest<bool>;

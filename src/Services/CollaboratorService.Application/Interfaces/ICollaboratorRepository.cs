using CollaboratorService.Domain.Entities;

namespace CollaboratorService.Application.Interfaces;

public interface ICollaboratorRepository
{
    Task AddAsync(Collaborator collaborator);

    Task RemoveAsync(int noteId, int collaboratorUserId);

    Task<bool> ExistsAsync(int noteId, int collaboratorUserId);
}
using MediatR;
using CollaboratorService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace CollaboratorService.Application.Features.Collaborators.Commands.RemoveCollaborator;

public class RemoveCollaboratorHandler : IRequestHandler<RemoveCollaboratorCommand, bool>
{
    private readonly ICollaboratorRepository _repo;

    public RemoveCollaboratorHandler(ICollaboratorRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(RemoveCollaboratorCommand request, CancellationToken cancellationToken)
    {
        if (request.NoteId <= 0)
            throw new BadRequestException("Invalid note id");

        if (request.CollaboratorUserId <= 0)
            throw new BadRequestException("Invalid collaborator user id");

        var exists = await _repo.ExistsAsync(request.NoteId, request.CollaboratorUserId);

        if (!exists)
            throw new NotFoundException("Collaborator not found");

        await _repo.RemoveAsync(request.NoteId, request.CollaboratorUserId);

        return true;
    }
}
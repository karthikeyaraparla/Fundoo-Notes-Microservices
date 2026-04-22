using MediatR;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Domain.Entities;
using SharedLibrary.CustomExceptions;
using CollaboratorService.Application.DTOs;

namespace CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;

public class AddCollaboratorHandler : IRequestHandler<AddCollaboratorCommand, bool>
{
    private readonly ICollaboratorRepository _repo;

    public AddCollaboratorHandler(ICollaboratorRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(AddCollaboratorCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (request.OwnerUserId <= 0)
            throw new BadRequestException("Invalid owner user id");

        if (request.Dto == null)
            throw new BadRequestException("Request body is missing");

        if (string.IsNullOrWhiteSpace(request.Dto.NoteId))
            throw new BadRequestException("NoteId is required");

        if (request.Dto.CollaboratorUserId <= 0)
            throw new BadRequestException("Invalid collaborator user id");

        if (request.OwnerUserId == request.Dto.CollaboratorUserId)
            throw new BadRequestException("Owner cannot be collaborator");

        // Create entity
        var collaborator = new Collaborator
        {
            NoteId = Convert.ToInt32(request.Dto.NoteId),
            OwnerUserId = request.OwnerUserId,
            CollaboratorUserId = request.Dto.CollaboratorUserId
        };

        await _repo.AddAsync(collaborator);

        return true;
    }
}
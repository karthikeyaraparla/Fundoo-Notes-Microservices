using MediatR;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Domain.Entities;
using SharedLibrary.CustomExceptions;
using CollaboratorService.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;

public class AddCollaboratorHandler : IRequestHandler<AddCollaboratorCommand, bool>
{
    private readonly ICollaboratorRepository _repo;
    private readonly ICollaboratorInvitationEmailService _emailService;
    private readonly ILogger<AddCollaboratorHandler> _logger;

    public AddCollaboratorHandler(
        ICollaboratorRepository repo,
        ICollaboratorInvitationEmailService emailService,
        ILogger<AddCollaboratorHandler> logger)
    {
        _repo = repo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(AddCollaboratorCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (request.OwnerUserId <= 0)
            throw new BadRequestException("Invalid owner user id");

        if (request.Dto == null)
            throw new BadRequestException("Request body is missing");

        if (request.Dto.NoteId <= 0)
            throw new BadRequestException("Invalid note id");

        if (request.Dto.CollaboratorUserId <= 0)
            throw new BadRequestException("Invalid collaborator user id");

        if (string.IsNullOrWhiteSpace(request.Dto.CollaboratorEmail))
            throw new BadRequestException("Collaborator email is required");

        if (request.OwnerUserId == request.Dto.CollaboratorUserId)
            throw new BadRequestException("Owner cannot be collaborator");

        // Create entity
        var collaborator = new Collaborator
        {
            NoteId = request.Dto.NoteId,
            OwnerUserId = request.OwnerUserId,
            CollaboratorUserId = request.Dto.CollaboratorUserId
        };

        await _repo.AddAsync(collaborator);

        try
        {
            await _emailService.SendCollaboratorInvitationAsync(
                request.Dto.CollaboratorEmail,
                request.Dto.NoteId,
                request.OwnerUserId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send collaborator invitation email for note {NoteId} to {CollaboratorEmail}",
                request.Dto.NoteId,
                request.Dto.CollaboratorEmail);
        }

        return true;
    }
}

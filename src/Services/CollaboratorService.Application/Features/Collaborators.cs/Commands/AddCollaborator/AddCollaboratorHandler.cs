using MediatR;
using CollaboratorService.Application.Events;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Application.Options;
using CollaboratorService.Domain.Entities;
using SharedLibrary.CustomExceptions;
using CollaboratorService.Application.DTOs;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;

public class AddCollaboratorHandler : IRequestHandler<AddCollaboratorCommand, bool>
{
    private readonly ICollaboratorRepository _repo;
    private readonly DaprClient _daprClient;
    private readonly CollaboratorInvitationPubSubOptions _pubSubOptions;
    private readonly ILogger<AddCollaboratorHandler> _logger;

    public AddCollaboratorHandler(
        ICollaboratorRepository repo,
        DaprClient daprClient,
        IOptions<CollaboratorInvitationPubSubOptions> pubSubOptions,
        ILogger<AddCollaboratorHandler> logger)
    {
        _repo = repo;
        _daprClient = daprClient;
        _pubSubOptions = pubSubOptions.Value;
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

        // Save collaborator
        var collaborator = new Collaborator
        {
            NoteId = request.Dto.NoteId,
            OwnerUserId = request.OwnerUserId,
            CollaboratorUserId = request.Dto.CollaboratorUserId
        };

        await _repo.AddAsync(collaborator);

        // Publish Event (Dapr Pub/Sub)
        try
        {
            var invitationEvent = new CollaboratorInvitationRequestedEvent(
                request.Dto.CollaboratorEmail,
                request.Dto.NoteId,
                request.OwnerUserId
            );

            await _daprClient.PublishEventAsync(
                _pubSubOptions.PubSubName,
                _pubSubOptions.TopicName,
                invitationEvent,
                cancellationToken
            );

            _logger.LogInformation(
                "Published event → NoteId: {NoteId}, Email: {Email}",
                request.Dto.NoteId,
                request.Dto.CollaboratorEmail
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event → NoteId: {NoteId}, Email: {Email}",
                request.Dto.NoteId,
                request.Dto.CollaboratorEmail
            );
        }

        return true;
    }
}
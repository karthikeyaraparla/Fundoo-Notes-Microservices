using CollaboratorService.Application.DTOs;
using MediatR;

namespace CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;

public record AddCollaboratorCommand(int OwnerUserId, AddCollaboratorDto Dto) : IRequest<bool>;
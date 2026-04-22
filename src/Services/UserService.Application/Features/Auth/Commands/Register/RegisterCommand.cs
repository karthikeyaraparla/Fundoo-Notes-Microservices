using MediatR;
using UserService.Application.DTOs;

// Command sent from controller → handler
public record RegisterCommand(RegisterDto Dto) : IRequest<bool>;
using MediatR;
using UserService.Application.DTOs;

// Login request → returns token
public record LoginCommand(LoginDto Dto) : IRequest<AuthResponseDto?>;
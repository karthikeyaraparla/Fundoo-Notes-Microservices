namespace UserService.Application.DTOs;

public record AuthResponseDto(
    string Token,
    string Email
);
namespace NotesService.Application.DTOs;

public record CreateNoteDto(
    string Title,
    string Description,
    string? Color
);
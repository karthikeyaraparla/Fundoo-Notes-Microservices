namespace NotesService.Application.DTOs;

public record UpdateNoteDto(
    int Id,
    string Title,
    string Description,
    string? Color
);

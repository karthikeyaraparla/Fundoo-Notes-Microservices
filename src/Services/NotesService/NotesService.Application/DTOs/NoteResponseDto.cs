namespace NotesService.Application.DTOs;

public record NoteResponseDto(
    int Id,
    string Title,
    string Description,
    string? Color,
    bool IsPinned,
    bool IsArchived,
    bool IsTrashed
);

namespace LabelsService.Application.DTOs;

public record LabeledNoteDto(
    int Id,
    string Title,
    string Description,
    string? Color,
    bool IsPinned,
    bool IsArchived,
    bool IsTrashed
);

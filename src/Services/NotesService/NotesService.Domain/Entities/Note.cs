namespace NotesService.Domain.Entities;

public class Note
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Color { get; set; }

    public bool IsPinned { get; set; } = false;

    public bool IsArchived { get; set; } = false;

    public bool IsTrashed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

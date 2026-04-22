namespace LabelsService.Domain.Entities;

public class NoteLabel
{
    public int Id { get; set; }

    public string NoteId { get; set; } = string.Empty;

    public int LabelId { get; set; }
}
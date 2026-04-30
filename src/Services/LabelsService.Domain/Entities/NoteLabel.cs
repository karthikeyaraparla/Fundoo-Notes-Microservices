namespace LabelsService.Domain.Entities;

public class NoteLabel
{
    public int Id { get; set; }

    public int NoteId { get; set; }

    public int LabelId { get; set; }
}

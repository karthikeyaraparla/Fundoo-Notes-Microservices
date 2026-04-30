using LabelsService.Domain.Entities;

namespace LabelsService.Application.Interfaces;

public interface ILabelRepository
{
    Task<int> CreateLabel(Label label);

    Task<bool> DeleteLabel(int labelId, int userId);

    Task<bool> AssignLabelToNote(int noteId, int labelId);

    Task<bool> RemoveLabelFromNote(int noteId, int labelId);

    Task<IEnumerable<Label>> GetLabelsByUser(int userId);

    Task<IReadOnlyList<int>> GetNoteIdsByLabelName(int userId, string labelName);
}

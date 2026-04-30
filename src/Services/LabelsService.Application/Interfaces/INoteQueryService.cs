using LabelsService.Application.DTOs;

namespace LabelsService.Application.Interfaces;

public interface INoteQueryService
{
    Task<IReadOnlyList<LabeledNoteDto>> GetNotesByIdsAsync(
        string authorizationHeader,
        IReadOnlyCollection<int> noteIds,
        CancellationToken cancellationToken);
}

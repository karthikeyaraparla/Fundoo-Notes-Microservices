using LabelsService.Application.Interfaces;
using MediatR;

namespace LabelsService.Application.Features.Labels.Commands.RemoveLabel;

public record RemoveLabelCommand(int NoteId, int LabelId) : IRequest<bool>;

public class RemoveLabelHandler : IRequestHandler<RemoveLabelCommand, bool>
{
    private readonly ILabelRepository _repo;

    public RemoveLabelHandler(ILabelRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(RemoveLabelCommand request, CancellationToken cancellationToken)
    {
        return await _repo.RemoveLabelFromNote(request.NoteId, request.LabelId);
    }
}

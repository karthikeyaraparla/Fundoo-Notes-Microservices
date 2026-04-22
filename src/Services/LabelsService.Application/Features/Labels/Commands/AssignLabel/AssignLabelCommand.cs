using LabelsService.Application.Interfaces;
using MediatR;

namespace LabelsService.Application.Features.Labels.Commands.AssignLabel;

public record AssignLabelCommand(string NoteId, int LabelId) : IRequest<bool>;

public class AssignLabelHandler : IRequestHandler<AssignLabelCommand, bool>
{
    private readonly ILabelRepository _repo;

    public AssignLabelHandler(ILabelRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(AssignLabelCommand request, CancellationToken cancellationToken)
    {
        return await _repo.AssignLabelToNote(request.NoteId, request.LabelId);
    }
}
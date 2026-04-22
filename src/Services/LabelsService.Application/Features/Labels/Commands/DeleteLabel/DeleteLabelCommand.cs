using LabelsService.Application.Interfaces;
using MediatR;

namespace LabelsService.Application.Features.Labels.Commands.DeleteLabel;

public record DeleteLabelCommand(int LabelId, int UserId) : IRequest<bool>;

public class DeleteLabelHandler : IRequestHandler<DeleteLabelCommand, bool>
{
    private readonly ILabelRepository _repo;

    public DeleteLabelHandler(ILabelRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
    {
        return await _repo.DeleteLabel(request.LabelId, request.UserId);
    }
}
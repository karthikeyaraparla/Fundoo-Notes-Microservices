using MediatR;
using LabelsService.Application.Interfaces;
using LabelsService.Domain.Entities;
using SharedLibrary.CustomExceptions;

namespace LabelsService.Application.Features.Labels.Commands;

public class CreateLabelHandler : IRequestHandler<CreateLabelCommand, int>
{
    private readonly ILabelRepository _repo;

    public CreateLabelHandler(ILabelRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (request.UserId <= 0)
            throw new BadRequestException("Invalid user id");

        if (request.Dto == null || string.IsNullOrWhiteSpace(request.Dto.Name))
            throw new BadRequestException("Label name is required");

        // Create entity
        var label = new Label
        {
            Name = request.Dto.Name,
            UserId = request.UserId
        };

        // Persist
        var id = await _repo.CreateLabel(label);

        if (id <= 0)
            throw new Exception("Failed to create label");

        return id;
    }
}
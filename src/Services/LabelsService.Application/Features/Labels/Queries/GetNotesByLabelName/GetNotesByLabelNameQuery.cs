using LabelsService.Application.DTOs;
using LabelsService.Application.Interfaces;
using MediatR;
using SharedLibrary.CustomExceptions;

namespace LabelsService.Application.Features.Labels.Queries.GetNotesByLabelName;

public record GetNotesByLabelNameQuery(
    int UserId,
    string LabelName,
    string AuthorizationHeader) : IRequest<IReadOnlyList<LabeledNoteDto>>;

public class GetNotesByLabelNameHandler : IRequestHandler<GetNotesByLabelNameQuery, IReadOnlyList<LabeledNoteDto>>
{
    private readonly ILabelRepository _labelRepository;
    private readonly INoteQueryService _noteQueryService;

    public GetNotesByLabelNameHandler(
        ILabelRepository labelRepository,
        INoteQueryService noteQueryService)
    {
        _labelRepository = labelRepository;
        _noteQueryService = noteQueryService;
    }

    public async Task<IReadOnlyList<LabeledNoteDto>> Handle(
        GetNotesByLabelNameQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
            throw new BadRequestException("Invalid user id");

        if (string.IsNullOrWhiteSpace(request.LabelName))
            throw new BadRequestException("Label name is required");

        if (string.IsNullOrWhiteSpace(request.AuthorizationHeader))
            throw new UnauthorizedException("Authorization header is required");

        var noteIds = await _labelRepository.GetNoteIdsByLabelName(request.UserId, request.LabelName);
        var distinctNoteIds = noteIds.Distinct().ToArray();

        if (distinctNoteIds.Length == 0)
            return Array.Empty<LabeledNoteDto>();

        return await _noteQueryService.GetNotesByIdsAsync(
            request.AuthorizationHeader,
            distinctNoteIds,
            cancellationToken);
    }
}

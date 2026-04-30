using MediatR;
using NotesService.Application.DTOs;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Queries.GetNotesByIds;

public record GetNotesByIdsQuery(int UserId, IReadOnlyCollection<int> NoteIds) : IRequest<List<NoteResponseDto>>;

public class GetNotesByIdsHandler : IRequestHandler<GetNotesByIdsQuery, List<NoteResponseDto>>
{
    private readonly INoteRepository _repo;

    public GetNotesByIdsHandler(INoteRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<NoteResponseDto>> Handle(GetNotesByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0)
            throw new BadRequestException("Invalid user id");

        if (request.NoteIds == null || request.NoteIds.Count == 0)
            return new List<NoteResponseDto>();

        var notes = await _repo.GetByIdsForUserAsync(request.UserId, request.NoteIds);

        return notes
            .Select(n => new NoteResponseDto(
                n.Id,
                n.Title,
                n.Description,
                n.Color,
                n.IsPinned,
                n.IsArchived,
                n.IsTrashed))
            .ToList();
    }
}

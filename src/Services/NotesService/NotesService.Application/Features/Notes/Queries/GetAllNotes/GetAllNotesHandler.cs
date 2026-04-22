using MediatR;
using NotesService.Application.DTOs;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Queries.GetAllNotes;

public class GetAllNotesHandler : IRequestHandler<GetAllNotesQuery, List<NoteResponseDto>>
{
    private readonly ICacheService _cache;
    private readonly INoteRepository _repo;

    public GetAllNotesHandler(INoteRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<List<NoteResponseDto>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        // Validation
        if (request.UserId <= 0)
            throw new BadRequestException("Invalid user id");

        var cacheKey = CacheKeys.NotesByUser(request.UserId);

        // Try cache
        try
        {
            var cached = await _cache.GetAsync<List<NoteResponseDto>>(cacheKey);

            if (cached != null)
                return cached;
        }
        catch
        {
            // Ignore cache failure
        }

        // Fetch from DB
        var notes = await _repo.GetByUserIdAsync(request.UserId);

        var result = notes.Select(n => new NoteResponseDto(
            n.Id,
            n.Title,
            n.Description,
            n.Color,
            n.IsPinned,
            n.IsArchived,
            n.IsTrashed
        )).ToList();

        // Store in cache safely
        try
        {
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        }
        catch
        {
            // Ignore cache failure
        }

        return result;
    }
}
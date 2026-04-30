using MediatR;
using Microsoft.Extensions.Logging;
using NotesService.Application.DTOs;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Queries.GetAllNotes;

public class GetAllNotesHandler : IRequestHandler<GetAllNotesQuery, List<NoteResponseDto>>
{
    private readonly ICacheService _cache;
    private readonly ILogger<GetAllNotesHandler> _logger;
    private readonly INoteRepository _repo;

    public GetAllNotesHandler(INoteRepository repo, ICacheService cache, ILogger<GetAllNotesHandler> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
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
            {
                _logger.LogInformation(
                    "Notes GET for user {UserId} served from cache using key {CacheKey}",
                    request.UserId,
                    cacheKey);
                return cached;
            }

            _logger.LogInformation(
                "Notes GET for user {UserId} cache miss for key {CacheKey}; loading from database",
                request.UserId,
                cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Notes GET for user {UserId} cache read failed for key {CacheKey}; loading from database",
                request.UserId,
                cacheKey);
        }

        // Fetch from DB
        _logger.LogInformation("Notes GET for user {UserId} reading from database", request.UserId);
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
            _logger.LogInformation(
                "Notes GET for user {UserId} stored {Count} notes in cache with key {CacheKey}",
                request.UserId,
                result.Count,
                cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Notes GET for user {UserId} cache write failed for key {CacheKey}",
                request.UserId,
                cacheKey);
        }

        return result;
    }
}

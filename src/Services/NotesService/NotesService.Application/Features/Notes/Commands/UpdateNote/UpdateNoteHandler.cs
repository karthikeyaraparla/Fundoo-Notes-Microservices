using MediatR;
using Microsoft.Extensions.Logging;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Commands.UpdateNote;

public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, bool>
{
    private readonly INoteRepository _repo;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateNoteHandler> _logger;

    public UpdateNoteHandler(INoteRepository repo, ICacheService cache, ILogger<UpdateNoteHandler> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (request.Dto.Id <= 0)
            throw new BadRequestException("Invalid note id");

        if (string.IsNullOrWhiteSpace(request.Dto.Title))
            throw new BadRequestException("Title is required");

        if (string.IsNullOrWhiteSpace(request.Dto.Description))
            throw new BadRequestException("Description is required");

        // Fetch note
        var note = await _repo.GetByIdAsync(request.Dto.Id);

        if (note == null)
            throw new NotFoundException("Note not found");

        // Update fields
        note.Title = request.Dto.Title;
        note.Description = request.Dto.Description;
        note.Color = request.Dto.Color;
        note.UpdatedAt = DateTime.UtcNow;

        // Persist changes
        await _repo.UpdateAsync(note);

        // Cache invalidation (safe)
        try
        {
            await _cache.RemoveAsync(CacheKeys.NotesByUser(note.UserId));
            _logger.LogInformation(
                "Notes cache invalidated for user {UserId} after note update",
                note.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Notes cache invalidation failed for user {UserId} after note update",
                note.UserId);
        }

        return true;
    }
}

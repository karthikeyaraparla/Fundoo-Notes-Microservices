using MediatR;
using Microsoft.Extensions.Logging;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Commands.DeleteNote;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly INoteRepository _repo;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteNoteHandler> _logger;

    public DeleteNoteHandler(INoteRepository repo, ICacheService cache, ILogger<DeleteNoteHandler> logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        //  VALIDATION
        if (request.Id <= 0)
            throw new BadRequestException("Invalid note id");

        //  FETCH NOTE
        var note = await _repo.GetByIdAsync(request.Id);

        if (note == null)
            throw new NotFoundException("Note not found");

        //  DELETE
        await _repo.DeleteAsync(request.Id);

        //  CACHE INVALIDATION (safe)
        try
        {
            await _cache.RemoveAsync(CacheKeys.NotesByUser(note.UserId));
            _logger.LogInformation(
                "Notes cache invalidated for user {UserId} after note deletion",
                note.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Notes cache invalidation failed for user {UserId} after note deletion",
                note.UserId);
        }

        return true;
    }
}

using MediatR;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Commands.DeleteNote;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly INoteRepository _repo;
    private readonly ICacheService _cache;

    public DeleteNoteHandler(INoteRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
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
        }
        catch
        {
            // Ignore cache failure
        }

        return true;
    }
}
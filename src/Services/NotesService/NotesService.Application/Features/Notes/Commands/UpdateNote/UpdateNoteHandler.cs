using MediatR;
using NotesService.Application.Interfaces;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Commands.UpdateNote;

public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, bool>
{
    private readonly INoteRepository _repo;
    private readonly ICacheService _cache;

    public UpdateNoteHandler(INoteRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
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
        }
        catch
        {
            // Ignore cache failure
        }

        return true;
    }
}
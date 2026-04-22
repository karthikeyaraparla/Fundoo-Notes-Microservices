using MediatR;
using NotesService.Application.Interfaces;
using NotesService.Domain.Entities;
using SharedLibrary.CustomExceptions;

namespace NotesService.Application.Features.Notes.Commands.CreateNote;

public class CreateNoteHandler : IRequestHandler<CreateNoteCommand, int>
{
    private readonly INoteRepository _repo;
    private readonly ICacheService _cache;

    public CreateNoteHandler(INoteRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<int> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        //  VALIDATION
        if (string.IsNullOrWhiteSpace(request.Dto.Title))
            throw new BadRequestException("Title is required");

        if (string.IsNullOrWhiteSpace(request.Dto.Description))
            throw new BadRequestException("Description is required");

        if (request.UserId <= 0)
            throw new UnauthorizedException("Invalid user");

        //  CREATE ENTITY
        var note = new Note
        {
            UserId = request.UserId,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            Color = request.Dto.Color
        };

        //  SAVE TO DB
        var id = await _repo.CreateAsync(note);

        if (id <= 0)
            throw new Exception("Failed to create note");

        //  CACHE INVALIDATION (safe)
        try
        {
            await _cache.RemoveAsync(CacheKeys.NotesByUser(request.UserId));
        }
        catch
        {
            // Don't break main flow if cache fails
        }

        return id;
    }
}
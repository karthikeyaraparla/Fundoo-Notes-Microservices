using MediatR;

namespace NotesService.Application.Features.Notes.Commands.DeleteNote;

public record DeleteNoteCommand(int Id) : IRequest<bool>;

using MediatR;
using NotesService.Application.DTOs;

namespace NotesService.Application.Features.Notes.Commands.UpdateNote;

public record UpdateNoteCommand(UpdateNoteDto Dto) : IRequest<bool>;

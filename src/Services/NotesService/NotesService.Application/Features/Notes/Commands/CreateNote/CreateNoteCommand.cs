using MediatR;
using NotesService.Application.DTOs;

namespace NotesService.Application.Features.Notes.Commands.CreateNote;

public record CreateNoteCommand(int UserId, CreateNoteDto Dto) : IRequest<int>;

using MediatR;
using NotesService.Application.DTOs;

namespace NotesService.Application.Features.Notes.Queries.GetAllNotes;

public record GetAllNotesQuery(int UserId) : IRequest<List<NoteResponseDto>>;

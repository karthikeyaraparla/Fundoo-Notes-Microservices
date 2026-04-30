using MediatR;
using NotesService.Application.DTOs;

public class GetNotesByIdsDto : IRequest<List<NoteResponseDto>>
{
    public List<int> NoteIds { get; set; } = new();
}
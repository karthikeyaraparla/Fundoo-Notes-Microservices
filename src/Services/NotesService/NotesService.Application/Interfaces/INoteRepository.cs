
using NotesService.Domain.Entities;

namespace NotesService.Application.Interfaces;

public interface INoteRepository
{
    Task<int> CreateAsync(Note note);
    Task<List<Note>> GetByUserIdAsync(int userId);
    Task<Note?> GetByIdAsync(int id);
    Task UpdateAsync(Note note);
    Task DeleteAsync(int id);
}

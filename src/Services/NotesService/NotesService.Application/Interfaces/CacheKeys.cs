namespace NotesService.Application.Interfaces;

public static class CacheKeys
{
    public static string NotesByUser(int userId) => $"notes:user:{userId}";
}

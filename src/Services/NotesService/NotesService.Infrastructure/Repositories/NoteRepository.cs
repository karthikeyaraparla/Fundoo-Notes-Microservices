using Dapper;
using NotesService.Application.Interfaces;
using NotesService.Domain.Entities;
using NotesService.Infrastructure.Persistence;

namespace NotesService.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly NotesDbContext _dbContext;

    public NoteRepository(NotesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateAsync(Note note)
    {
        const string query = """
            INSERT INTO Notes (UserId, Title, Description, Color, IsPinned, IsArchived, IsTrashed, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @Title, @Description, @Color, @IsPinned, @IsArchived, @IsTrashed, @CreatedAt, @UpdatedAt);
            """;

        using var connection = _dbContext.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(query, note);
    }

    public async Task<List<Note>> GetByUserIdAsync(int userId)
    {
        const string query = """
            SELECT Id, UserId, Title, Description, Color, IsPinned, IsArchived, IsTrashed, CreatedAt, UpdatedAt
            FROM Notes
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;
            """;

        using var connection = _dbContext.CreateConnection();
        var notes = await connection.QueryAsync<Note>(query, new { UserId = userId });
        return notes.ToList();
    }

    public async Task<Note?> GetByIdAsync(int id)
    {
        const string query = """
            SELECT Id, UserId, Title, Description, Color, IsPinned, IsArchived, IsTrashed, CreatedAt, UpdatedAt
            FROM Notes
            WHERE Id = @Id;
            """;

        using var connection = _dbContext.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Note>(query, new { Id = id });
    }

    public async Task UpdateAsync(Note note)
    {
        const string query = """
            UPDATE Notes
            SET Title = @Title,
                Description = @Description,
                Color = @Color,
                IsPinned = @IsPinned,
                IsArchived = @IsArchived,
                IsTrashed = @IsTrashed,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
            """;

        using var connection = _dbContext.CreateConnection();
        await connection.ExecuteAsync(query, note);
    }

    public async Task DeleteAsync(int id)
    {
        const string query = "DELETE FROM Notes WHERE Id = @Id;";

        using var connection = _dbContext.CreateConnection();
        await connection.ExecuteAsync(query, new { Id = id });
    }
}

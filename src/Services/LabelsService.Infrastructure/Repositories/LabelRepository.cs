using Dapper;
using LabelsService.Application.Interfaces;
using LabelsService.Infrastructure.Persistence;
using LabelsService.Domain.Entities;
using Label = LabelsService.Domain.Entities.Label;

namespace LabelsService.Infrastructure.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly DbConnectionFactory _factory;

    public LabelRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> CreateLabel(Label label)
    {
        using var conn = _factory.CreateConnection();

        string query = @"INSERT INTO Labels (Name, UserId)
                         VALUES (@Name, @UserId);
                         SELECT CAST(SCOPE_IDENTITY() as int);";

        return await conn.ExecuteScalarAsync<int>(query, label);
    }

    public async Task<bool> DeleteLabel(int labelId, int userId)
    {
        using var conn = _factory.CreateConnection();

        string query = "DELETE FROM Labels WHERE Id=@labelId AND UserId=@userId";

        return await conn.ExecuteAsync(query, new { labelId, userId }) > 0;
    }

    public async Task<bool> AssignLabelToNote(int noteId, int labelId)
    {
        using var conn = _factory.CreateConnection();

        string query = "INSERT INTO NoteLabels (NoteId, LabelId) VALUES (@noteId, @labelId)";

        return await conn.ExecuteAsync(query, new { noteId, labelId }) > 0;
    }

    public async Task<bool> RemoveLabelFromNote(int noteId, int labelId)
    {
        using var conn = _factory.CreateConnection();

        string query = "DELETE FROM NoteLabels WHERE NoteId=@noteId AND LabelId=@labelId";

        return await conn.ExecuteAsync(query, new { noteId, labelId }) > 0;
    }

    public async Task<IEnumerable<Label>> GetLabelsByUser(int userId)
    {
        using var conn = _factory.CreateConnection();

        string query = "SELECT * FROM Labels WHERE UserId=@userId";

        return await conn.QueryAsync<Label>(query, new { userId });
    }

    public async Task<IReadOnlyList<int>> GetNoteIdsByLabelName(int userId, string labelName)
    {
        using var conn = _factory.CreateConnection();

        const string query = """
            SELECT DISTINCT nl.NoteId
            FROM NoteLabels nl
            INNER JOIN Labels l ON l.Id = nl.LabelId
            WHERE l.UserId = @userId
              AND l.Name LIKE @labelName
            ORDER BY nl.NoteId DESC;
            """;

        var noteIds = await conn.QueryAsync<int>(query, new
        {
            userId,
            labelName = $"%{labelName}%"
        });

        return noteIds.ToList();
    }
}

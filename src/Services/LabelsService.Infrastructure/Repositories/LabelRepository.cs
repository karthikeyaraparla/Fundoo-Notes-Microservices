using Dapper;
using LabelsService.Domain.Entities;
using LabelsService.Application.Interfaces;
using LabelsService.Infrastructure.Persistence;
using Microsoft.Data.SqlClient.DataClassification;
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

    public async Task<bool> AssignLabelToNote(string noteId, int labelId)
    {
        using var conn = _factory.CreateConnection();

        string query = "INSERT INTO NoteLabels (NoteId, LabelId) VALUES (@noteId, @labelId)";

        return await conn.ExecuteAsync(query, new { noteId, labelId }) > 0;
    }

    public async Task<bool> RemoveLabelFromNote(string noteId, int labelId)
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
}
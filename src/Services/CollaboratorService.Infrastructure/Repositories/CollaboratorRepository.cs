using Dapper;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Domain.Entities;
using CollaboratorService.Infrastructure.Persistence;

namespace CollaboratorService.Infrastructure.Repositories;

public class CollaboratorRepository : ICollaboratorRepository
{
    private readonly DbConnectionFactory _factory;

    public CollaboratorRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task AddAsync(Collaborator collaborator)
    {
        var query = @"
            INSERT INTO Collaborators (NoteId, OwnerUserId, CollaboratorUserId)
            VALUES (@NoteId, @OwnerUserId, @CollaboratorUserId);
        ";

        using var connection = _factory.CreateConnection();

        await connection.ExecuteAsync(query, collaborator);
    }

    public async Task RemoveAsync(int noteId, int collaboratorUserId)
    {
        var query = @"
            DELETE FROM Collaborators
            WHERE NoteId = @NoteId AND CollaboratorUserId = @CollaboratorUserId;
        ";

        using var connection = _factory.CreateConnection();

        await connection.ExecuteAsync(query, new { NoteId = noteId, CollaboratorUserId = collaboratorUserId });
    }

    // ✅ ADD THIS METHOD
    public async Task<bool> ExistsAsync(int noteId, int collaboratorUserId)
    {
        var query = @"
            SELECT COUNT(1)
            FROM Collaborators
            WHERE NoteId = @NoteId AND CollaboratorUserId = @CollaboratorUserId;
        ";

        using var connection = _factory.CreateConnection();

        var count = await connection.ExecuteScalarAsync<int>(
            query,
            new { NoteId = noteId, CollaboratorUserId = collaboratorUserId }
        );

        return count > 0;
    }
}
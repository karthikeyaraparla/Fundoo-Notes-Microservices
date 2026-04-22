using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CollaboratorService.Infrastructure.Persistence;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task EnsureDatabaseObjectsAsync()
    {
        using var connection = CreateConnection();

        // STEP 1: Create DB 
        await connection.ExecuteAsync(@"
            IF DB_ID('FundooUserDb') IS NULL
                CREATE DATABASE FundooUserDb;
        ");

        // STEP 2: Switch to DB
        await connection.ExecuteAsync("USE FundooUserDb;");

        // STEP 3: Create Collaborators table
        const string sql = @"
            IF OBJECT_ID('dbo.Collaborators', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Collaborators
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    NoteId INT NOT NULL,
                    OwnerUserId INT NOT NULL,
                    CollaboratorUserId INT NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
            END;
        ";

        await connection.ExecuteAsync(sql);
    }
}
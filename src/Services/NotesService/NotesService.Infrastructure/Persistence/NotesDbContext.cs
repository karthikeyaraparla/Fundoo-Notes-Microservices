using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace NotesService.Infrastructure.Persistence;

public class NotesDbContext
{
    private readonly string _connectionString;

    public NotesDbContext(string connectionString)
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

        // STEP 1: Create Database if not exists
        await connection.ExecuteAsync(@"
            IF DB_ID('FundooUserDb') IS NULL
                CREATE DATABASE FundooUserDb;
        ");

        // STEP 2: Switch to DB
        await connection.ExecuteAsync("USE FundooUserDb;");

        // STEP 3: Create Notes table
        const string sql = @"
            IF OBJECT_ID('dbo.Notes', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Notes
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NOT NULL,
                    Title NVARCHAR(200) NOT NULL,
                    Description NVARCHAR(MAX) NOT NULL,
                    Color NVARCHAR(50) NULL,
                    IsPinned BIT NOT NULL DEFAULT 0,
                    IsArchived BIT NOT NULL DEFAULT 0,
                    IsTrashed BIT NOT NULL DEFAULT 0,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                    UpdatedAt DATETIME2 NULL
                );
            END;
        ";

        await connection.ExecuteAsync(sql);
    }
}
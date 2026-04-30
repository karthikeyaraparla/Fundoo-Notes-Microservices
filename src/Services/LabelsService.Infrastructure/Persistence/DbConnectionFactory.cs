using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace LabelsService.Infrastructure.Persistence;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);

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

        // STEP 3: Create Labels table
        var query = @"
            IF OBJECT_ID('dbo.Labels', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Labels (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    UserId INT NOT NULL
                );
            END;

            IF OBJECT_ID('dbo.NoteLabels', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.NoteLabels (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    NoteId INT NOT NULL,
                    LabelId INT NOT NULL
                );
            END;
        ";

        await connection.ExecuteAsync(query);
    }
}

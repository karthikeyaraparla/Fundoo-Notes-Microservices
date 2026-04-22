using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace UserService.Infrastructure.Persistence;

// Responsible for creating DB connections
public class UserDbContext
{
    private readonly string _connectionString;

    public UserDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Returns SQL connection
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

        // STEP 2: Switch to database
        await connection.ExecuteAsync("USE FundooUserDb;");

        // STEP 3: Create Users table (your existing logic)
        const string sql = @"
            IF OBJECT_ID('dbo.Users', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users
                (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    FirstName NVARCHAR(100) NOT NULL,
                    LastName NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(255) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(255) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
            END;
        ";

        await connection.ExecuteAsync(sql);
    }
}
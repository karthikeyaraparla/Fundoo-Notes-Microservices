using Dapper;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _factory;

    public UserRepository(UserDbContext factory)
    {
        _factory = factory;
    }

    // Insert user into SQL
    public async Task CreateAsync(User user)
    {
        var query = @"
            INSERT INTO Users (FirstName, LastName, Email, PasswordHash)
            VALUES (@FirstName, @LastName, @Email, @PasswordHash);
        ";

        using var connection = _factory.CreateConnection();

        await connection.ExecuteAsync(query, user);
    }

    // Get user by email
    public async Task<User?> GetByEmailAsync(string email)
    {
        var query = @"
            SELECT * FROM Users WHERE Email = @Email;
        ";

        using var connection = _factory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
    }
}
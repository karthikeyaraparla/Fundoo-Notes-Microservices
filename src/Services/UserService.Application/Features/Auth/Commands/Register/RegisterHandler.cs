using MediatR;
using UserService.Domain.Entities;
using SharedLibrary.CustomExceptions;

namespace UserService.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly IUserRepository _repo;

    public RegisterHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 🔍 VALIDATION
        if (string.IsNullOrWhiteSpace(request.Dto.FirstName))
            throw new BadRequestException("First name is required");

        if (string.IsNullOrWhiteSpace(request.Dto.Email))
            throw new BadRequestException("Email is required");

        if (string.IsNullOrWhiteSpace(request.Dto.Password))
            throw new BadRequestException("Password is required");

        if (request.Dto.Password.Length < 6)
            throw new BadRequestException("Password must be at least 6 characters");

        // 🔍 CHECK DUPLICATE USER
        var existingUser = await _repo.GetByEmailAsync(request.Dto.Email);

        if (existingUser != null)
            throw new BadRequestException("User already exists");

        // 🔐 HASH PASSWORD
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Dto.Password);

        // 🧱 CREATE ENTITY
        var user = new User
        {
            FirstName = request.Dto.FirstName,
            LastName = request.Dto.LastName,
            Email = request.Dto.Email,
            PasswordHash = hashedPassword
        };

        // 💾 SAVE TO DB
        await _repo.CreateAsync(user);

        // ✅ SUCCESS
        return true;
    }
}
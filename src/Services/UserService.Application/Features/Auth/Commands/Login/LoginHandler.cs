using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.DTOs;
using SharedLibrary.CustomExceptions;

namespace UserService.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _repo;
    private readonly IConfiguration _configuration;

    public LoginHandler(IUserRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 🔍 Find user
        var user = await _repo.GetByEmailAsync(request.Dto.Email);

        if (user == null)
            throw new UnauthorizedException("Invalid email or password");

        // 🔐 Verify password
        bool isValid = BCrypt.Net.BCrypt.Verify(
            request.Dto.Password,
            user.PasswordHash
        );

        if (!isValid)
            throw new UnauthorizedException("Invalid email or password");

        // 🔐 JWT Generation
        var tokenHandler = new JwtSecurityTokenHandler();

        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new Exception("JWT Secret not configured");

        var issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new Exception("JWT Issuer not configured");

        var audience = _configuration["JwtSettings:Audience"]
            ?? throw new Exception("JWT Audience not configured");

        var key = Encoding.UTF8.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 🔥 IMPORTANT FIX
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Issuer = issuer,
            Audience = audience,
            Expires = DateTime.UtcNow.AddHours(2),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new AuthResponseDto(
            tokenHandler.WriteToken(token),
            user.Email
        );
    }
}
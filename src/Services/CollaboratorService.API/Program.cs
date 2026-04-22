using System.Text;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Infrastructure.Persistence;
using CollaboratorService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// CONFIG

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is required.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer is required.");

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience is required.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

// SERVICES

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Collaborator Service API",
        Version = "v1"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter 'Bearer {token}'",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [jwtSecurityScheme] = Array.Empty<string>()
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AddCollaboratorHandler).Assembly));

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

// DATABASE

builder.Services.AddSingleton<DbConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new DbConnectionFactory(config.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();

// APP

var app = builder.Build();


// 🔥 FIXED RETRY LOGIC (IMPORTANT)

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    int maxRetries = 20; // 🔥 increased retries
    int delay = 5000;    // 5 seconds

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            await dbFactory.EnsureDatabaseObjectsAsync();
            logger.LogInformation("✅ Database initialized successfully");
            break;
        }
        catch (SqlException ex)
        {
            logger.LogWarning($"⚠️ Attempt {i}/{maxRetries} - SQL not ready... retrying in 5 seconds");

            if (i == maxRetries)
            {
                logger.LogError(ex, "❌ Database failed after retries — continuing app startup");
                break; // 🔥 DO NOT CRASH
            }

            await Task.Delay(delay);
        }
    }
}


// MIDDLEWARE

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseGlobalExceptionMiddleware();

// ⚠️ Disable HTTPS in Docker (important)
 // app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
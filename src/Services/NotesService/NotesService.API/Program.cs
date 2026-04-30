using System.Text;
using Dapr.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotesService.Application.Features.Notes.Commands.CreateNote;
using NotesService.Application.Interfaces;
using NotesService.Infrastructure.Persistence;
using NotesService.Infrastructure.Repositories;
using NotesService.Infrastructure.Cache;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// CONFIG

var daprHttpEndpoint = builder.Configuration["Dapr:HttpEndpoint"];
var daprStateStoreName = builder.Configuration["Dapr:StateStoreName"];

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
        Title = "Notes Service API",
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
    cfg.RegisterServicesFromAssembly(typeof(CreateNoteHandler).Assembly));

// JWT
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

// CACHE
builder.Services.AddMemoryCache();
builder.Services.AddLogging();
builder.Services.AddSingleton(_ =>
{
    var clientBuilder = new DaprClientBuilder();

    if (!string.IsNullOrWhiteSpace(daprHttpEndpoint))
    {
        clientBuilder.UseHttpEndpoint(daprHttpEndpoint);
    }

    return clientBuilder.Build();
});

// DATABASE
builder.Services.AddSingleton(new NotesDbContext(connectionString));
builder.Services.AddScoped<INoteRepository, NoteRepository>();

// CACHE SERVICE
builder.Services.AddSingleton<ICacheService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Cache");

    if (!string.IsNullOrWhiteSpace(daprHttpEndpoint) && !string.IsNullOrWhiteSpace(daprStateStoreName))
    {
        try
        {
            logger.LogInformation("Using Dapr state store cache for NotesService.");
            return new DaprStateCacheService(
                serviceProvider.GetRequiredService<DaprClient>(),
                daprStateStoreName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Dapr state store is unavailable, using memory cache.");
        }
    }

    return new MemoryCacheService(serviceProvider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>());
});

var app = builder.Build();


//  FIXED RETRY LOGIC (VERY IMPORTANT)

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    int maxRetries = 20;
    int delay = 5000;

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            await dbContext.EnsureDatabaseObjectsAsync();
            logger.LogInformation(" Notes DB initialized successfully");
            break;
        }
        catch (SqlException ex)
        {
            logger.LogWarning($" Attempt {i}/{maxRetries} - SQL not ready... retrying");

            if (i == maxRetries)
            {
                logger.LogError(ex, " Notes DB failed — continuing startup");
                break; //  DO NOT CRASH
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

// Disable HTTPS in Docker
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using System.Text;
using CollaboratorService.Application.Features.Collaborators.Commands.AddCollaborator;
using CollaboratorService.Application.Interfaces;
using CollaboratorService.Application.Options;
using CollaboratorService.Infrastructure.Persistence;
using CollaboratorService.Infrastructure.Repositories;
using Dapr.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

#region CONFIG

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

var pubSubName = builder.Configuration["Dapr:PubSubName"] ?? "rabbitmq-pubsub";
var topicName = builder.Configuration["Dapr:CollaboratorInvitationTopic"] ?? "collaborator-invitations";

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is required.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer is required.");

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience is required.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

#endregion

#region SERVICES

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Collaborator Service API",
        Version = "v1"
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AddCollaboratorHandler).Assembly));

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

// PubSub Options
builder.Services.Configure<CollaboratorInvitationPubSubOptions>(options =>
{
    options.PubSubName = pubSubName;
    options.TopicName = topicName;
});

// Dapr Client
builder.Services.AddDaprClient();

// Database (FIXED)
builder.Services.AddSingleton<DbConnectionFactory>(sp =>
{
    return new DbConnectionFactory(connectionString);
});

builder.Services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();

#endregion

var app = builder.Build();

#region DB INIT

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    int maxRetries = 20;
    int delay = 5000;

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            await dbFactory.EnsureDatabaseObjectsAsync();
            logger.LogInformation("Database initialized successfully");
            break;
        }
        catch (SqlException)
        {
            logger.LogWarning("SQL not ready... retrying");
            await Task.Delay(delay);
        }
    }
}

#endregion

#region MIDDLEWARE

app.UseRouting();

app.UseCloudEvents();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseGlobalExceptionMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapSubscribeHandler();

app.MapControllers();

#endregion

app.Run();
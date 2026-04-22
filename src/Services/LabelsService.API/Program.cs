using LabelsService.Application.Features.Labels.Commands;
using LabelsService.Application.Interfaces;
using LabelsService.Infrastructure.Persistence;
using LabelsService.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);

// CONTROLLERS
builder.Services.AddControllers();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// MEDIATR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateLabelCommand).Assembly));

// DATABASE
builder.Services.AddSingleton<DbConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new DbConnectionFactory(config.GetConnectionString("DefaultConnection")!);
});

// REPOSITORY
builder.Services.AddScoped<ILabelRepository, LabelRepository>();

// JWT AUTH
var jwtKey = builder.Configuration["Jwt:Key"] ?? "THIS_IS_A_SECRET_KEY_CHANGE_IT";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


//  FIXED DATABASE RETRY LOGIC

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    int maxRetries = 20;   // increased retries
    int delay = 5000;      // 5 sec delay

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            await dbFactory.EnsureDatabaseObjectsAsync();
            logger.LogInformation(" Labels DB initialized successfully");
            break;
        }
        catch (SqlException ex)
        {
            logger.LogWarning($"⚠ Attempt {i}/{maxRetries} - SQL not ready... retrying in 5 seconds");

            if (i == maxRetries)
            {
                logger.LogError(ex, " DB failed after retries — continuing startup");
                break; // DO NOT CRASH
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

// IMPORTANT: disable HTTPS in Docker
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
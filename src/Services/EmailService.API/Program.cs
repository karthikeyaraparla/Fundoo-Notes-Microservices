using EmailService.API.Models;
using EmailService.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Dapr
builder.Services.AddControllers().AddDapr();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dapr Client
builder.Services.AddDaprClient();

// SMTP Config
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    return new SmtpSettings
    {
        Host = config["SmtpSettings:Host"] ?? "",
        Port = int.TryParse(config["SmtpSettings:Port"], out var port) ? port : 0,
        Username = config["SmtpSettings:Username"] ?? "",
        Password = config["SmtpSettings:Password"] ?? "",
        FromEmail = config["SmtpSettings:FromEmail"] ?? "",
        FromName = config["SmtpSettings:FromName"] ?? "Fundoo Notes",
        EnableSsl = bool.TryParse(config["SmtpSettings:EnableSsl"], out var ssl) && ssl
    };
});

builder.Services.AddScoped<ICollaboratorInvitationEmailSender, SmtpCollaboratorInvitationEmailSender>();

var app = builder.Build();

app.UseRouting();            // ✅ REQUIRED
app.UseCloudEvents();        // ✅ REQUIRED

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// ✅ REQUIRED
app.MapSubscribeHandler();

app.MapControllers();

app.Run();
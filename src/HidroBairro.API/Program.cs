using HidroBairro.API.Endpoints;
using HidroBairro.API.Extensions;
using HidroBairro.Application;
using HidroBairro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Render (e outros PaaS) definem PORT; local continua 5080 via launchSettings.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHidroBairroAuth(builder.Configuration);
builder.Services.AddHidroBairroSwagger(builder.Configuration);
builder.Services.AddHidroBairroRateLimiting(builder.Configuration);
builder.Services.AddHidroBairroHealthChecks(builder.Configuration);

var app = builder.Build();

// Development local OU Render com ASPNETCORE_ENVIRONMENT=Development (Swagger + demos).
if (app.Environment.IsDevelopment())
{
    app.UseHidroBairroSwagger();
}

app.UseRateLimiter();
app.UseHidroBairroAuth();

app.MapGet("/", () => Results.Ok(new
{
    service = "HidroBairro.API",
    boundedContext = "Zeladoria",
    status = "ok",
    swagger = "/swagger",
    health = new { live = "/health/live", ready = "/health/ready" },
    auth = new { devToken = "/api/auth/dev-token" }
}))
.AllowAnonymous();

app.MapHidroBairroHealthChecks();
app.MapAuthEndpoints();
app.MapCategoriaEndpoints();
app.MapSolicitacaoEndpoints();

app.Run();

public partial class Program;

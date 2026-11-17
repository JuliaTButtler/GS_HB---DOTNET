using HidroBairro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HidroBairro.API.Extensions;

public static class HealthCheckExtensions
{
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";

    public static IServiceCollection AddHidroBairroHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("API process is up."),
                tags: [LiveTag])
            .AddDbContextCheck<AppDbContext>(
                name: "oracle",
                failureStatus: HealthStatus.Unhealthy,
                tags: [ReadyTag]);

        var javaUrl = configuration["HealthChecks:JavaApiUrl"];
        if (!string.IsNullOrWhiteSpace(javaUrl))
        {
            healthChecks.AddCheck(
                "java-api",
                new ExternalHttpHealthCheck(javaUrl),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ReadyTag]);
        }

        return services;
    }

    public static WebApplication MapHidroBairroHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(LiveTag),
                ResponseWriter = WriteMinimalJson
            })
            .AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(ReadyTag),
                ResponseWriter = WriteDetailedJson
            })
            .AllowAnonymous();

        return app;
    }

    private static Task WriteMinimalJson(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        });

    private static Task WriteDetailedJson(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
                error = e.Value.Exception?.Message
            })
        });
}

/// <summary>
/// Checa um serviço HTTP externo (ex.: API Java). Só entra no ready se HealthChecks:JavaApiUrl estiver configurado.
/// </summary>
internal sealed class ExternalHttpHealthCheck : IHealthCheck
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private readonly string _url;

    public ExternalHttpHealthCheck(string url)
    {
        _url = url;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync(_url, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Reached {_url} ({(int)response.StatusCode}).")
                : HealthCheckResult.Unhealthy($"External service returned {(int)response.StatusCode} for {_url}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Cannot reach {_url}.", ex);
        }
    }
}

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace HidroBairro.API.Extensions;

public static class RateLimitingExtensions
{
    public const string ApiPolicyName = "api";

    public static IServiceCollection AddHidroBairroRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;
                response.ContentType = "application/json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await response.WriteAsJsonAsync(
                    new
                    {
                        error = "Too Many Requests",
                        message = "Limite de requisições excedido. Tente novamente em breve.",
                        status = 429
                    },
                    cancellationToken);
            };

            options.AddPolicy(ApiPolicyName, httpContext =>
            {
                var opts = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimitingOptions>>()
                    .Value;

                var partitionKey =
                    httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? httpContext.Request.Headers[DevUserExtensions.UserIdHeader].FirstOrDefault()
                    ?? "anon";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, opts.PermitLimit),
                        Window = TimeSpan.FromSeconds(Math.Max(1, opts.WindowSeconds)),
                        QueueLimit = Math.Max(0, opts.QueueLimit),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
            });
        });

        return services;
    }
}

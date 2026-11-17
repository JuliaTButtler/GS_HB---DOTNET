using System.Text;
using HidroBairro.API.Auth;
using HidroBairro.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HidroBairro.API.Extensions;

public static class AuthExtensions
{
    public const string RoleClaimType = "role";
    public const string SubClaimType = "sub";

    public static IServiceCollection AddHidroBairroAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        if (!auth.Enabled)
            return services;

        if (string.IsNullOrWhiteSpace(auth.Key) || auth.Key.Length < 32)
            throw new InvalidOperationException(
                "Auth:Key deve ter pelo menos 32 caracteres quando Auth:Enabled=true.");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = auth.Issuer,
                    ValidateAudience = true,
                    ValidAudience = auth.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = RoleClaimType,
                    NameClaimType = SubClaimType
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.Cidadao, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(PapelUsuario.Cidadao));

            options.AddPolicy(AuthPolicies.Gestor, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(PapelUsuario.Gestor));

            options.AddPolicy(AuthPolicies.CidadaoOuGestor, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(PapelUsuario.Cidadao, PapelUsuario.Gestor));
        });

        return services;
    }

    public static WebApplication UseHidroBairroAuth(this WebApplication app)
    {
        var auth = app.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
                   ?? new AuthOptions();

        if (!auth.Enabled)
            return app;

        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static bool IsAuthEnabled(this IEndpointRouteBuilder app)
    {
        var auth = app.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthOptions>>().Value;
        return auth.Enabled;
    }
}

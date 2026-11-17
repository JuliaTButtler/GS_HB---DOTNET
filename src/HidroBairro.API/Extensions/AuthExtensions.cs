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

        var signingKey = JwtKeyHelper.CreateSigningKey(auth);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = auth.ValidateIssuer,
                    ValidIssuer = auth.ValidateIssuer ? auth.Issuer : null,
                    ValidateAudience = auth.ValidateAudience,
                    ValidAudience = auth.ValidateAudience ? auth.Audience : null,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
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

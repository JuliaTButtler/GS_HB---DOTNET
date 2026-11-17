using HidroBairro.API.Auth;
using Microsoft.OpenApi.Models;

namespace HidroBairro.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddHidroBairroSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HidroBairro Sul — Zeladoria",
                Version = "v1",
                Description = auth.Enabled
                    ? "API .NET de solicitações de zeladoria. Auth: JWT Bearer. " +
                      (auth.LocalIssuerEnabled
                          ? "Dev: POST /api/auth/dev-token para emitir token local. "
                          : "Issuer oficial: API Java. ") +
                      "POST /api/solicitacoes exige Idempotency-Key."
                    : "API .NET de solicitações de zeladoria. Auth desligada — use headers X-User-Id / X-User-Role. " +
                      "POST /api/solicitacoes exige Idempotency-Key."
            });

            if (auth.Enabled)
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT no header. Ex.: Bearer eyJhbGciOi...",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }
            else
            {
                options.AddSecurityDefinition("DevUser", new OpenApiSecurityScheme
                {
                    Description = "Usuário de desenvolvimento (sem JWT). Ex.: 1",
                    Name = DevUserExtensions.UserIdHeader,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityDefinition("DevRole", new OpenApiSecurityScheme
                {
                    Description = "Papel de desenvolvimento: CIDADAO ou GESTOR",
                    Name = DevUserExtensions.UserRoleHeader,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "DevUser"
                            }
                        },
                        Array.Empty<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "DevRole"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        });

        return services;
    }

    public static WebApplication UseHidroBairroSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "HidroBairro Zeladoria v1");
            options.DocumentTitle = "HidroBairro — Swagger";
        });

        return app;
    }
}

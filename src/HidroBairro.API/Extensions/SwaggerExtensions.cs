using Microsoft.OpenApi.Models;

namespace HidroBairro.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddHidroBairroSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HidroBairro Sul — Zeladoria",
                Version = "v1",
                Description =
                    "API .NET de solicitações de zeladoria. " +
                    "Em Development: sem JWT — use headers X-User-Id e X-User-Role. " +
                    "POST /api/solicitacoes exige Idempotency-Key."
            });

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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HidroBairro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext Oracle, repositórios e Unit of Work entram na etapa de persistência.
        _ = configuration;
        return services;
    }
}

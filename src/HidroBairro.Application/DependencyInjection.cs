using Microsoft.Extensions.DependencyInjection;

namespace HidroBairro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services (Solicitacao, Categoria, etc.) serão registrados nas próximas etapas.
        return services;
    }
}

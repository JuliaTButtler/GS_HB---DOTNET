using HidroBairro.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HidroBairro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<ISolicitacaoService, SolicitacaoService>();
        return services;
    }
}

using HidroBairro.Application.Abstractions;
using HidroBairro.Infrastructure.Persistence;
using HidroBairro.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HidroBairro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OracleConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseOracle(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ISolicitacaoRepository, SolicitacaoRepository>();
        services.AddScoped<IHistoricoStatusRepository, HistoricoStatusRepository>();

        return services;
    }
}

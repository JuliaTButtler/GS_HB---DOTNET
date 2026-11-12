using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HidroBairro.Infrastructure.Persistence;

/// <summary>
/// Factory para `dotnet ef` (migrations). Lê a connection string da API.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "HidroBairro.API"));

        if (!Directory.Exists(apiPath))
        {
            apiPath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "src", "HidroBairro.API"));
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("OracleConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:OracleConnection não configurada. " +
                "Preencha em src/HidroBairro.API/appsettings.Development.json");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseOracle(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}

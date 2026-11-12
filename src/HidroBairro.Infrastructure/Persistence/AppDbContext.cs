using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroBairro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<CategoriaSolicitacao> Categorias => Set<CategoriaSolicitacao>();
    public DbSet<SolicitacaoServico> Solicitacoes => Set<SolicitacaoServico>();
    public DbSet<HistoricoStatus> HistoricosStatus => Set<HistoricoStatus>();
    public DbSet<UsuarioRef> Usuarios => Set<UsuarioRef>();
    public DbSet<PontoMonitoramentoRef> Pontos => Set<PontoMonitoramentoRef>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

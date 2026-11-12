using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabela Java existente — mapeada só para FK; fora das migrations .NET.
/// </summary>
public class PontoMonitoramentoRefConfiguration : IEntityTypeConfiguration<PontoMonitoramentoRef>
{
    public void Configure(EntityTypeBuilder<PontoMonitoramentoRef> builder)
    {
        builder.ToTable("PONTO_MONITORAMENTO_HB", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(120).IsUnicode(false).IsRequired();
        builder.Property(x => x.Subprefeitura).HasColumnName("SUBPREFEITURA").HasMaxLength(80).IsUnicode(false).IsRequired();
        builder.Property(x => x.Bairro).HasColumnName("BAIRRO").HasMaxLength(80).IsUnicode(false).IsRequired();
        builder.Property(x => x.Zona).HasColumnName("ZONA").HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("LATITUDE").HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("LONGITUDE").HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("ATIVO").HasMaxLength(1).IsUnicode(false).IsRequired();
    }
}

using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tabela Java existente — mapeada só para FK; fora das migrations .NET.
/// </summary>
public class UsuarioRefConfiguration : IEntityTypeConfiguration<UsuarioRef>
{
    public void Configure(EntityTypeBuilder<UsuarioRef> builder)
    {
        builder.ToTable("USUARIO_HB", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ID");
        builder.Property(x => x.Nome).HasColumnName("NOME").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(180).IsRequired();
        builder.Property(x => x.Papel).HasColumnName("PAPEL").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("ATIVO").HasMaxLength(1).IsRequired();
    }
}

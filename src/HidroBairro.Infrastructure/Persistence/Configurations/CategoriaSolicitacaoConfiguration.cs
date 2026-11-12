using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class CategoriaSolicitacaoConfiguration : IEntityTypeConfiguration<CategoriaSolicitacao>
{
    public void Configure(EntityTypeBuilder<CategoriaSolicitacao> builder)
    {
        builder.ToTable("CATEGORIA_SOLICITACAO_HB", t =>
        {
            t.HasCheckConstraint("CHK_CATEGORIA_HB_ATIVO", "ATIVO IN ('S', 'N')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(255);

        builder.Property(x => x.Ativo)
            .HasColumnName("ATIVO")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue(AtivoFlags.Sim);

        builder.HasIndex(x => x.Nome)
            .IsUnique()
            .HasDatabaseName("UQ_CATEGORIA_HB_NOME");
    }
}

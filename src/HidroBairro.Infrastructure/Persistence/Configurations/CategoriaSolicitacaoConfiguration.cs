using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class CategoriaSolicitacaoConfiguration : IEntityTypeConfiguration<CategoriaSolicitacao>
{
    public void Configure(EntityTypeBuilder<CategoriaSolicitacao> builder)
    {
        // CHECK constraints ficam na migration via Sql() — HasCheckConstraint quebra o EXECUTE IMMEDIATE do Oracle.
        builder.ToTable("CATEGORIA_SOLICITACAO_HB");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(80)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(x => x.Ativo)
            .HasColumnName("ATIVO")
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue(AtivoFlags.Sim);

        builder.HasIndex(x => x.Nome)
            .IsUnique()
            .HasDatabaseName("UQ_CATEGORIA_HB_NOME");
    }
}

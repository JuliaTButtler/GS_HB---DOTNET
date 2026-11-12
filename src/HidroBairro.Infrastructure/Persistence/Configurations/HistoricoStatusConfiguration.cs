using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class HistoricoStatusConfiguration : IEntityTypeConfiguration<HistoricoStatus>
{
    public void Configure(EntityTypeBuilder<HistoricoStatus> builder)
    {
        // CHECK constraints ficam na migration via Sql() — HasCheckConstraint quebra o EXECUTE IMMEDIATE do Oracle.
        builder.ToTable("HISTORICO_STATUS_HB");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SolicitacaoId)
            .HasColumnName("SOLICITACAO_ID")
            .IsRequired();

        builder.Property(x => x.StatusAnterior)
            .HasColumnName("STATUS_ANTERIOR")
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(x => x.StatusNovo)
            .HasColumnName("STATUS_NOVO")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.UsuarioId)
            .HasColumnName("USUARIO_ID")
            .IsRequired();

        builder.Property(x => x.Observacao)
            .HasColumnName("OBSERVACAO")
            .HasMaxLength(500)
            .IsUnicode(false);

        builder.Property(x => x.CriadoEm)
            .HasColumnName("CRIADO_EM")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .HasConstraintName("FK_HIST_HB_USUARIO")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

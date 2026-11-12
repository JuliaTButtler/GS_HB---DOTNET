using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class HistoricoStatusConfiguration : IEntityTypeConfiguration<HistoricoStatus>
{
    public void Configure(EntityTypeBuilder<HistoricoStatus> builder)
    {
        builder.ToTable("HISTORICO_STATUS_HB", t =>
        {
            t.HasCheckConstraint(
                "CHK_HIST_HB_STATUS_NOVO",
                "STATUS_NOVO IN ('RECEBIDA', 'EM_VISTORIA', 'CONCLUIDA', 'CANCELADA')");

            t.HasCheckConstraint(
                "CHK_HIST_HB_STATUS_ANT",
                "STATUS_ANTERIOR IS NULL OR STATUS_ANTERIOR IN ('RECEBIDA', 'EM_VISTORIA', 'CONCLUIDA', 'CANCELADA')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SolicitacaoId)
            .HasColumnName("SOLICITACAO_ID")
            .IsRequired();

        builder.Property(x => x.StatusAnterior)
            .HasColumnName("STATUS_ANTERIOR")
            .HasMaxLength(20);

        builder.Property(x => x.StatusNovo)
            .HasColumnName("STATUS_NOVO")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.UsuarioId)
            .HasColumnName("USUARIO_ID")
            .IsRequired();

        builder.Property(x => x.Observacao)
            .HasColumnName("OBSERVACAO")
            .HasMaxLength(500);

        builder.Property(x => x.CriadoEm)
            .HasColumnName("CRIADO_EM")
            .IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .HasConstraintName("FK_HIST_HB_USUARIO")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

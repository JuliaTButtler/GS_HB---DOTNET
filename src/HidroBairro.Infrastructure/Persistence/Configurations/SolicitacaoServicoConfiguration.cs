using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class SolicitacaoServicoConfiguration : IEntityTypeConfiguration<SolicitacaoServico>
{
    public void Configure(EntityTypeBuilder<SolicitacaoServico> builder)
    {
        builder.ToTable("SOLICITACAO_SERVICO_HB", t =>
        {
            t.HasCheckConstraint(
                "CHK_SOLICITACAO_HB_STATUS",
                "STATUS IN ('RECEBIDA', 'EM_VISTORIA', 'CONCLUIDA', 'CANCELADA')");

            t.HasCheckConstraint(
                "CHK_SOLICITACAO_HB_LOCAL",
                "PONTO_ID IS NOT NULL OR (LATITUDE IS NOT NULL AND LONGITUDE IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Protocolo)
            .HasColumnName("PROTOCOLO")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.UsuarioId)
            .HasColumnName("USUARIO_ID")
            .IsRequired();

        builder.Property(x => x.CategoriaId)
            .HasColumnName("CATEGORIA_ID")
            .IsRequired();

        builder.Property(x => x.PontoId)
            .HasColumnName("PONTO_ID");

        builder.Property(x => x.Titulo)
            .HasColumnName("TITULO")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasColumnName("LATITUDE")
            .HasPrecision(10, 7);

        builder.Property(x => x.Longitude)
            .HasColumnName("LONGITUDE")
            .HasPrecision(10, 7);

        builder.Property(x => x.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(SolicitacaoStatus.Recebida);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("IDEMPOTENCY_KEY")
            .HasMaxLength(80);

        builder.Property(x => x.CriadoEm)
            .HasColumnName("CRIADO_EM")
            .IsRequired();

        builder.Property(x => x.AtualizadoEm)
            .HasColumnName("ATUALIZADO_EM")
            .IsRequired();

        builder.HasIndex(x => x.Protocolo)
            .IsUnique()
            .HasDatabaseName("UQ_SOLICITACAO_HB_PROTOCOLO");

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UQ_SOLICITACAO_HB_IDEMPOTENCY");

        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.Solicitacoes)
            .HasForeignKey(x => x.CategoriaId)
            .HasConstraintName("FK_SOLICITACAO_HB_CATEGORIA")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .HasConstraintName("FK_SOLICITACAO_HB_USUARIO")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Ponto)
            .WithMany()
            .HasForeignKey(x => x.PontoId)
            .HasConstraintName("FK_SOLICITACAO_HB_PONTO")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(x => x.Historicos)
            .WithOne(h => h.Solicitacao)
            .HasForeignKey(h => h.SolicitacaoId)
            .HasConstraintName("FK_HIST_HB_SOLICITACAO")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

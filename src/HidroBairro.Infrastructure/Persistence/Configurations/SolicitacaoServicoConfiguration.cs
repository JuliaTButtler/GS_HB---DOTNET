using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HidroBairro.Infrastructure.Persistence.Configurations;

public class SolicitacaoServicoConfiguration : IEntityTypeConfiguration<SolicitacaoServico>
{
    public void Configure(EntityTypeBuilder<SolicitacaoServico> builder)
    {
        // CHECK constraints ficam na migration via Sql() — HasCheckConstraint quebra o EXECUTE IMMEDIATE do Oracle.
        builder.ToTable("SOLICITACAO_SERVICO_HB");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Protocolo)
            .HasColumnName("PROTOCOLO")
            .HasMaxLength(30)
            .IsUnicode(false)
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
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(1000)
            .IsUnicode(false)
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
            .IsUnicode(false)
            .IsRequired()
            .HasDefaultValue(SolicitacaoStatus.Recebida);

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("IDEMPOTENCY_KEY")
            .HasMaxLength(80)
            .IsUnicode(false);

        builder.Property(x => x.CriadoEm)
            .HasColumnName("CRIADO_EM")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.AtualizadoEm)
            .HasColumnName("ATUALIZADO_EM")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
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

namespace HidroBairro.Domain.Models;

/// <summary>
/// Trilha de mudança de status do chamado. Tabela: HISTORICO_STATUS_HB.
/// </summary>
public class HistoricoStatus
{
    public long Id { get; set; }

    public long SolicitacaoId { get; set; }

    public string? StatusAnterior { get; set; }

    public string StatusNovo { get; set; } = string.Empty;

    /// <summary>FK para USUARIO_HB — quem alterou o status.</summary>
    public long UsuarioId { get; set; }

    public string? Observacao { get; set; }

    public DateTime CriadoEm { get; set; }

    public SolicitacaoServico? Solicitacao { get; set; }

    public UsuarioRef? Usuario { get; set; }
}

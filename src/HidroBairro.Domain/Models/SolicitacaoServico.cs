namespace HidroBairro.Domain.Models;

/// <summary>
/// Chamado de zeladoria aberto pelo cidadão. Tabela: SOLICITACAO_SERVICO_HB.
/// </summary>
public class SolicitacaoServico
{
    public long Id { get; set; }

    public string Protocolo { get; set; } = string.Empty;

    /// <summary>FK para USUARIO_HB (domínio Java).</summary>
    public long UsuarioId { get; set; }

    public long CategoriaId { get; set; }

    /// <summary>FK opcional para PONTO_MONITORAMENTO_HB (domínio Java).</summary>
    public long? PontoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string Status { get; set; } = SolicitacaoStatus.Recebida;

    public string? IdempotencyKey { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime AtualizadoEm { get; set; }

    public CategoriaSolicitacao? Categoria { get; set; }

    public UsuarioRef? Usuario { get; set; }

    public PontoMonitoramentoRef? Ponto { get; set; }

    public ICollection<HistoricoStatus> Historicos { get; set; } = new List<HistoricoStatus>();
}

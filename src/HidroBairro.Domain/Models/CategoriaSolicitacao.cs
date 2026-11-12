namespace HidroBairro.Domain.Models;

/// <summary>
/// Categoria de chamado de zeladoria. Tabela: CATEGORIA_SOLICITACAO_HB.
/// </summary>
public class CategoriaSolicitacao
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    /// <summary>Oracle CHAR(1): 'S' ou 'N'.</summary>
    public string Ativo { get; set; } = AtivoFlags.Sim;

    public ICollection<SolicitacaoServico> Solicitacoes { get; set; } = new List<SolicitacaoServico>();
}

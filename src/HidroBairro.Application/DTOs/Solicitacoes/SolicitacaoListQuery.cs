namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class SolicitacaoListQuery
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Status { get; init; }

    /// <summary>
    /// Se true, lista só do usuário autenticado (cidadão).
    /// Gestor pode passar false para ver a fila.
    /// </summary>
    public bool SomenteDoUsuario { get; init; } = true;
}

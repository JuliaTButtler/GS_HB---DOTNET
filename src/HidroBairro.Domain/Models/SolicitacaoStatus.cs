namespace HidroBairro.Domain.Models;

/// <summary>Valores do CHECK CHK_SOLICITACAO_HB_STATUS / histórico.</summary>
public static class SolicitacaoStatus
{
    public const string Recebida = "RECEBIDA";
    public const string EmVistoria = "EM_VISTORIA";
    public const string Concluida = "CONCLUIDA";
    public const string Cancelada = "CANCELADA";

    public static readonly IReadOnlySet<string> Todos = new HashSet<string>(StringComparer.Ordinal)
    {
        Recebida,
        EmVistoria,
        Concluida,
        Cancelada
    };

    public static bool IsValid(string? status) =>
        !string.IsNullOrWhiteSpace(status) && Todos.Contains(status);
}

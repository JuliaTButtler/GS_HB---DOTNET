namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class HistoricoStatusDto
{
    public long Id { get; init; }
    public long SolicitacaoId { get; init; }
    public string? StatusAnterior { get; init; }
    public string StatusNovo { get; init; } = string.Empty;
    public long UsuarioId { get; init; }
    public string? Observacao { get; init; }
    public DateTime CriadoEm { get; init; }
}

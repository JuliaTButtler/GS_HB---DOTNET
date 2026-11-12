namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class SolicitacaoDto
{
    public long Id { get; init; }
    public string Protocolo { get; init; } = string.Empty;
    public long UsuarioId { get; init; }
    public long CategoriaId { get; init; }
    public string? CategoriaNome { get; init; }
    public long? PontoId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? IdempotencyKey { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
}

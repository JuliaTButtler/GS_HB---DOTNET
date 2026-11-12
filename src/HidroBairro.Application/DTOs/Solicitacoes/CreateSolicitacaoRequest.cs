namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class CreateSolicitacaoRequest
{
    public long CategoriaId { get; init; }
    public long? PontoId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
}

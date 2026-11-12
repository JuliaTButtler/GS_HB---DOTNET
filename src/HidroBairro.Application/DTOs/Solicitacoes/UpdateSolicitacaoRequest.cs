namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class UpdateSolicitacaoRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
}

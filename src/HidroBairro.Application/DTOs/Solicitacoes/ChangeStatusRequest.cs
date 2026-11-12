namespace HidroBairro.Application.DTOs.Solicitacoes;

public sealed class ChangeStatusRequest
{
    public string StatusNovo { get; init; } = string.Empty;
    public string? Observacao { get; init; }
}

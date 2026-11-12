using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Abstractions;

public interface ISolicitacaoRepository
{
    Task<SolicitacaoServico?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<SolicitacaoServico?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<SolicitacaoServico> Items, int Total)> ListAsync(
        long? usuarioId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(SolicitacaoServico solicitacao, CancellationToken cancellationToken = default);
    void Update(SolicitacaoServico solicitacao);
}

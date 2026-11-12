using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Abstractions;

public interface IHistoricoStatusRepository
{
    Task AddAsync(HistoricoStatus historico, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HistoricoStatus>> ListBySolicitacaoIdAsync(
        long solicitacaoId,
        CancellationToken cancellationToken = default);
}

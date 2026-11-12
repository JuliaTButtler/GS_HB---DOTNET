using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Abstractions;

public interface ICategoriaRepository
{
    Task<CategoriaSolicitacao?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaSolicitacao>> ListAtivasAsync(CancellationToken cancellationToken = default);
}

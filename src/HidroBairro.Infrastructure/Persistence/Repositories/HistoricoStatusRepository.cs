using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroBairro.Infrastructure.Persistence.Repositories;

public sealed class HistoricoStatusRepository : IHistoricoStatusRepository
{
    private readonly AppDbContext _db;

    public HistoricoStatusRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(HistoricoStatus historico, CancellationToken cancellationToken = default)
    {
        await _db.HistoricosStatus.AddAsync(historico, cancellationToken);
    }

    public async Task<IReadOnlyList<HistoricoStatus>> ListBySolicitacaoIdAsync(
        long solicitacaoId,
        CancellationToken cancellationToken = default)
    {
        return await _db.HistoricosStatus
            .AsNoTracking()
            .Where(h => h.SolicitacaoId == solicitacaoId)
            .OrderBy(h => h.CriadoEm)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);
    }
}

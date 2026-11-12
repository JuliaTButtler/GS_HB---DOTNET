using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Models;

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
}

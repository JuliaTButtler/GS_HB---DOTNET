using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroBairro.Infrastructure.Persistence.Repositories;

public sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _db;

    public CategoriaRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<CategoriaSolicitacao?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CategoriaSolicitacao>> ListAtivasAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Categorias
            .AsNoTracking()
            .Where(c => c.Ativo == AtivoFlags.Sim)
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }
}

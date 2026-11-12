using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroBairro.Infrastructure.Persistence.Repositories;

public sealed class SolicitacaoRepository : ISolicitacaoRepository
{
    private readonly AppDbContext _db;

    public SolicitacaoRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<SolicitacaoServico?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _db.Solicitacoes
            .Include(s => s.Categoria)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<SolicitacaoServico?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        _db.Solicitacoes
            .AsNoTracking()
            .Include(s => s.Categoria)
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<(IReadOnlyList<SolicitacaoServico> Items, int Total)> ListAsync(
        long? usuarioId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Solicitacoes
            .AsNoTracking()
            .Include(s => s.Categoria)
            .AsQueryable();

        if (usuarioId is not null)
            query = query.Where(s => s.UsuarioId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.Status == status);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(SolicitacaoServico solicitacao, CancellationToken cancellationToken = default)
    {
        await _db.Solicitacoes.AddAsync(solicitacao, cancellationToken);
    }

    public void Update(SolicitacaoServico solicitacao)
    {
        _db.Solicitacoes.Update(solicitacao);
    }
}

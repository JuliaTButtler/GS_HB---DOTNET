using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HidroBairro.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRefRepository : IUsuarioRefRepository
{
    private readonly AppDbContext _db;

    public UsuarioRefRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<UsuarioRef?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UsuarioRef?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        // Match exato do EMAIL em USUARIO_HB (como gravado pelo Java).
        return _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }
}

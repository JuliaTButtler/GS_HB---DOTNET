using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Abstractions;

public interface IUsuarioRefRepository
{
    Task<UsuarioRef?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UsuarioRef?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

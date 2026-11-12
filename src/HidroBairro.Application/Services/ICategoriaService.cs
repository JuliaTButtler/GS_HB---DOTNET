using HidroBairro.Application.Common;
using HidroBairro.Application.DTOs.Categorias;

namespace HidroBairro.Application.Services;

public interface ICategoriaService
{
    Task<ServiceResult<IReadOnlyList<CategoriaDto>>> ListAtivasAsync(
        CancellationToken cancellationToken = default);
}

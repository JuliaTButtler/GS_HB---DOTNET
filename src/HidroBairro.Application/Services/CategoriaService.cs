using HidroBairro.Application.Abstractions;
using HidroBairro.Application.Common;
using HidroBairro.Application.DTOs.Categorias;
using HidroBairro.Application.Mapping;

namespace HidroBairro.Application.Services;

public sealed class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categorias;

    public CategoriaService(ICategoriaRepository categorias)
    {
        _categorias = categorias;
    }

    public async Task<ServiceResult<IReadOnlyList<CategoriaDto>>> ListAtivasAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _categorias.ListAtivasAsync(cancellationToken);
        var dtos = items.Select(CategoriaMapper.ToDto).ToList();
        return ServiceResult<IReadOnlyList<CategoriaDto>>.Success(dtos);
    }
}

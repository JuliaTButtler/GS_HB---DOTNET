using HidroBairro.Application.DTOs.Categorias;
using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Mapping;

public static class CategoriaMapper
{
    public static CategoriaDto ToDto(CategoriaSolicitacao entity) => new()
    {
        Id = entity.Id,
        Nome = entity.Nome,
        Descricao = entity.Descricao,
        Ativo = entity.Ativo
    };
}

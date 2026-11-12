using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Mapping;

public static class SolicitacaoMapper
{
    public static SolicitacaoDto ToDto(SolicitacaoServico entity) => new()
    {
        Id = entity.Id,
        Protocolo = entity.Protocolo,
        UsuarioId = entity.UsuarioId,
        CategoriaId = entity.CategoriaId,
        CategoriaNome = entity.Categoria?.Nome,
        PontoId = entity.PontoId,
        Titulo = entity.Titulo,
        Descricao = entity.Descricao,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        Status = entity.Status,
        IdempotencyKey = entity.IdempotencyKey,
        CriadoEm = entity.CriadoEm,
        AtualizadoEm = entity.AtualizadoEm
    };
}

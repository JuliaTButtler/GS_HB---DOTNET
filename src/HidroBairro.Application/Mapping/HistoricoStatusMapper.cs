using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Domain.Models;

namespace HidroBairro.Application.Mapping;

public static class HistoricoStatusMapper
{
    public static HistoricoStatusDto ToDto(HistoricoStatus entity) => new()
    {
        Id = entity.Id,
        SolicitacaoId = entity.SolicitacaoId,
        StatusAnterior = entity.StatusAnterior,
        StatusNovo = entity.StatusNovo,
        UsuarioId = entity.UsuarioId,
        Observacao = entity.Observacao,
        CriadoEm = entity.CriadoEm
    };
}

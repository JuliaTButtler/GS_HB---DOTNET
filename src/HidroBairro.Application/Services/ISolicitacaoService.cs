using HidroBairro.Application.Common;
using HidroBairro.Application.DTOs.Solicitacoes;

namespace HidroBairro.Application.Services;

public interface ISolicitacaoService
{
    Task<ServiceResult<SolicitacaoDto>> CreateAsync(
        CreateSolicitacaoRequest request,
        long usuarioId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<SolicitacaoDto>> GetByIdAsync(
        long id,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<HistoricoStatusDto>>> ListHistoricoAsync(
        long id,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<SolicitacaoDto>>> ListAsync(
        SolicitacaoListQuery query,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<SolicitacaoDto>> UpdateAsync(
        long id,
        UpdateSolicitacaoRequest request,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<SolicitacaoDto>> CancelAsync(
        long id,
        long usuarioId,
        string papel,
        string? observacao = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<SolicitacaoDto>> ChangeStatusAsync(
        long id,
        ChangeStatusRequest request,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default);
}

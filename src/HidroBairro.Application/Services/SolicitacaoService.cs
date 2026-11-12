using HidroBairro.Application.Abstractions;
using HidroBairro.Application.Common;
using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Application.Mapping;
using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using HidroBairro.Domain.Rules;

namespace HidroBairro.Application.Services;

public sealed class SolicitacaoService : ISolicitacaoService
{
    private readonly ISolicitacaoRepository _solicitacoes;
    private readonly ICategoriaRepository _categorias;
    private readonly IHistoricoStatusRepository _historicos;
    private readonly IUnitOfWork _unitOfWork;

    public SolicitacaoService(
        ISolicitacaoRepository solicitacoes,
        ICategoriaRepository categorias,
        IHistoricoStatusRepository historicos,
        IUnitOfWork unitOfWork)
    {
        _solicitacoes = solicitacoes;
        _categorias = categorias;
        _historicos = historicos;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<SolicitacaoDto>> CreateAsync(
        CreateSolicitacaoRequest request,
        long usuarioId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IdempotencyRules.EnsureKeyValida(idempotencyKey);
            var key = IdempotencyRules.Normalize(idempotencyKey);

            var existente = await _solicitacoes.GetByIdempotencyKeyAsync(key, cancellationToken);
            if (existente is not null)
                return ServiceResult<SolicitacaoDto>.Success(SolicitacaoMapper.ToDto(existente));

            SolicitacaoRules.EnsureUsuarioInformado(usuarioId);
            SolicitacaoRules.EnsureCategoriaInformada(request.CategoriaId);
            SolicitacaoRules.EnsureTituloValido(request.Titulo);
            SolicitacaoRules.EnsureDescricaoValida(request.Descricao);
            SolicitacaoRules.EnsureLocalInformado(request.PontoId, request.Latitude, request.Longitude);

            var categoria = await _categorias.GetByIdAsync(request.CategoriaId, cancellationToken);
            if (categoria is null)
                return ServiceResult<SolicitacaoDto>.AsNotFound($"Categoria '{request.CategoriaId}' não encontrada.");

            CategoriaRules.EnsureAtiva(categoria.Ativo);

            var agora = DateTime.UtcNow;
            var solicitacao = new SolicitacaoServico
            {
                Protocolo = GerarProtocolo(),
                UsuarioId = usuarioId,
                CategoriaId = request.CategoriaId,
                PontoId = request.PontoId is > 0 ? request.PontoId : null,
                Titulo = request.Titulo.Trim(),
                Descricao = request.Descricao.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Status = SolicitacaoStatus.Recebida,
                IdempotencyKey = key,
                CriadoEm = agora,
                AtualizadoEm = agora,
                Categoria = categoria
            };

            await _solicitacoes.AddAsync(solicitacao, cancellationToken);
            await _historicos.AddAsync(new HistoricoStatus
            {
                Solicitacao = solicitacao,
                StatusAnterior = null,
                StatusNovo = SolicitacaoStatus.Recebida,
                UsuarioId = usuarioId,
                Observacao = "Abertura da solicitação",
                CriadoEm = agora
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<SolicitacaoDto>.CreatedSuccess(SolicitacaoMapper.ToDto(solicitacao));
        }
        catch (BusinessRuleException ex)
        {
            return ServiceResult<SolicitacaoDto>.Failure(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return ServiceResult<SolicitacaoDto>.AsForbidden(ex.Message);
        }
    }

    public async Task<ServiceResult<SolicitacaoDto>> GetByIdAsync(
        long id,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await _solicitacoes.GetByIdAsync(id, cancellationToken);
        if (solicitacao is null)
            return ServiceResult<SolicitacaoDto>.AsNotFound($"Solicitação '{id}' não encontrada.");

        if (!PodeVisualizar(solicitacao, usuarioId, papel))
            return ServiceResult<SolicitacaoDto>.AsForbidden("Você não pode visualizar esta solicitação.");

        return ServiceResult<SolicitacaoDto>.Success(SolicitacaoMapper.ToDto(solicitacao));
    }

    public async Task<ServiceResult<PagedResult<SolicitacaoDto>>> ListAsync(
        SolicitacaoListQuery query,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default)
    {
        var (page, pageSize) = PagingHelper.Normalize(query.Page, query.PageSize);

        long? filtroUsuario = null;
        if (string.Equals(papel, PapelUsuario.Cidadao, StringComparison.Ordinal) || query.SomenteDoUsuario)
            filtroUsuario = usuarioId;

        if (string.Equals(papel, PapelUsuario.Gestor, StringComparison.Ordinal) && !query.SomenteDoUsuario)
            filtroUsuario = null;

        var (items, total) = await _solicitacoes.ListAsync(
            filtroUsuario,
            query.Status,
            page,
            pageSize,
            cancellationToken);

        var result = new PagedResult<SolicitacaoDto>
        {
            Items = items.Select(SolicitacaoMapper.ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };

        return ServiceResult<PagedResult<SolicitacaoDto>>.Success(result);
    }

    public async Task<ServiceResult<SolicitacaoDto>> UpdateAsync(
        long id,
        UpdateSolicitacaoRequest request,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var solicitacao = await _solicitacoes.GetByIdAsync(id, cancellationToken);
            if (solicitacao is null)
                return ServiceResult<SolicitacaoDto>.AsNotFound($"Solicitação '{id}' não encontrada.");

            if (!EhDono(solicitacao, usuarioId) && !EhGestor(papel))
                return ServiceResult<SolicitacaoDto>.AsForbidden("Você não pode editar esta solicitação.");

            SolicitacaoRules.EnsureEditavelPorCidadao(solicitacao.Status);
            SolicitacaoRules.EnsureTituloValido(request.Titulo);
            SolicitacaoRules.EnsureDescricaoValida(request.Descricao);

            solicitacao.Titulo = request.Titulo.Trim();
            solicitacao.Descricao = request.Descricao.Trim();
            solicitacao.AtualizadoEm = DateTime.UtcNow;
            _solicitacoes.Update(solicitacao);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<SolicitacaoDto>.Success(SolicitacaoMapper.ToDto(solicitacao));
        }
        catch (BusinessRuleException ex)
        {
            return ServiceResult<SolicitacaoDto>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<SolicitacaoDto>> CancelAsync(
        long id,
        long usuarioId,
        string papel,
        string? observacao = null,
        CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(
            id,
            new ChangeStatusRequest
            {
                StatusNovo = SolicitacaoStatus.Cancelada,
                Observacao = observacao ?? "Cancelada pelo solicitante"
            },
            usuarioId,
            papel,
            cancellationToken);
    }

    public async Task<ServiceResult<SolicitacaoDto>> ChangeStatusAsync(
        long id,
        ChangeStatusRequest request,
        long usuarioId,
        string papel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var solicitacao = await _solicitacoes.GetByIdAsync(id, cancellationToken);
            if (solicitacao is null)
                return ServiceResult<SolicitacaoDto>.AsNotFound($"Solicitação '{id}' não encontrada.");

            if (string.Equals(papel, PapelUsuario.Cidadao, StringComparison.Ordinal) &&
                !EhDono(solicitacao, usuarioId))
            {
                return ServiceResult<SolicitacaoDto>.AsForbidden("Você não pode alterar esta solicitação.");
            }

            var statusNovo = request.StatusNovo.Trim().ToUpperInvariant();
            StatusTransitionRules.EnsurePapelPodeAlterar(papel, solicitacao.Status, statusNovo);

            var anterior = solicitacao.Status;
            var agora = DateTime.UtcNow;
            solicitacao.Status = statusNovo;
            solicitacao.AtualizadoEm = agora;
            _solicitacoes.Update(solicitacao);

            await _historicos.AddAsync(new HistoricoStatus
            {
                SolicitacaoId = solicitacao.Id,
                Solicitacao = solicitacao,
                StatusAnterior = anterior,
                StatusNovo = solicitacao.Status,
                UsuarioId = usuarioId,
                Observacao = request.Observacao,
                CriadoEm = agora
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult<SolicitacaoDto>.Success(SolicitacaoMapper.ToDto(solicitacao));
        }
        catch (BusinessRuleException ex)
        {
            return ServiceResult<SolicitacaoDto>.Failure(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return ServiceResult<SolicitacaoDto>.AsForbidden(ex.Message);
        }
    }

    private static string GerarProtocolo()
    {
        // HB-yyyyMMdd-xxxxxxxx (≤ 30 chars)
        return $"HB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}".ToUpperInvariant();
    }

    private static bool EhDono(SolicitacaoServico s, long usuarioId) => s.UsuarioId == usuarioId;

    private static bool EhGestor(string papel) =>
        string.Equals(papel, PapelUsuario.Gestor, StringComparison.Ordinal);

    private static bool PodeVisualizar(SolicitacaoServico s, long usuarioId, string papel) =>
        EhGestor(papel) || EhDono(s, usuarioId);
}

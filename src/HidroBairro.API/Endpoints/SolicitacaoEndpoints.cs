using HidroBairro.API.Auth;
using HidroBairro.API.Extensions;
using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Application.Services;
using HidroBairro.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HidroBairro.API.Endpoints;

public static class SolicitacaoEndpoints
{
    public static RouteGroupBuilder MapSolicitacaoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/solicitacoes")
            .WithTags("Solicitacoes")
            .RequireRateLimiting(RateLimitingExtensions.ApiPolicyName);

        if (app.IsAuthEnabled())
            group.RequireAuthorization(AuthPolicies.CidadaoOuGestor);

        group.MapPost("/", async (
            CreateSolicitacaoRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return Results.BadRequest(new
                {
                    error = "Header Idempotency-Key é obrigatório."
                });
            }

            try
            {
                var usuarioId = await http.GetUsuarioIdAsync(cancellationToken);
                var result = await service.CreateAsync(
                    request,
                    usuarioId,
                    idempotencyKey,
                    cancellationToken);

                if (result.Succeeded && result.Created && result.Data is not null)
                    return result.ToHttpResult($"/api/solicitacoes/{result.Data.Id}");

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("CreateSolicitacao")
        .WithSummary("Abre solicitação (Idempotency-Key obrigatório)")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? status,
            [FromQuery] bool? somenteDoUsuario,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var query = new SolicitacaoListQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    SomenteDoUsuario = somenteDoUsuario ?? true
                };

                var result = await service.ListAsync(
                    query,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("ListSolicitacoes")
        .WithSummary("Lista solicitações (cidadão: as dele; gestor: fila se somenteDoUsuario=false)");

        group.MapGet("/{id:long}", async (
            long id,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.GetByIdAsync(
                    id,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("GetSolicitacao")
        .WithSummary("Busca solicitação por id")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:long}/historico", async (
            long id,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.ListHistoricoAsync(
                    id,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("ListSolicitacaoHistorico")
        .WithSummary("Lista histórico de status da solicitação (ordem cronológica)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:long}", async (
            long id,
            UpdateSolicitacaoRequest request,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.UpdateAsync(
                    id,
                    request,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("UpdateSolicitacao")
        .WithSummary("Edita título/descrição (só RECEBIDA)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:long}/cancelar", async (
            long id,
            [FromQuery] string? observacao,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.CancelAsync(
                    id,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    observacao,
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("CancelSolicitacao")
        .WithSummary("Cancela solicitação")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:long}/status", async (
            long id,
            ChangeStatusRequest request,
            HttpContext http,
            ISolicitacaoService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.ChangeStatusAsync(
                    id,
                    request,
                    await http.GetUsuarioIdAsync(cancellationToken),
                    http.GetPapel(),
                    cancellationToken);

                return result.ToHttpResult();
            }
            catch (UnauthorizedException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithName("ChangeSolicitacaoStatus")
        .WithSummary("Altera status (gestor: fluxo operacional; cidadão: cancelar a própria)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}

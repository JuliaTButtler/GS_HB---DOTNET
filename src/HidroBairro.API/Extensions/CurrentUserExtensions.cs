using System.Security.Claims;
using HidroBairro.API.Auth;
using HidroBairro.Application.Abstractions;
using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using Microsoft.Extensions.Options;

namespace HidroBairro.API.Extensions;

/// <summary>
/// Resolve usuário/papel: JWT Java (sub=email + role) ou headers DevUser (Auth desligado).
/// </summary>
public static class CurrentUserExtensions
{
    public static async Task<long> GetUsuarioIdAsync(
        this HttpContext http,
        CancellationToken cancellationToken = default)
    {
        var auth = http.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;

        if (!auth.Enabled)
            return DevUserExtensions.GetUsuarioIdFromHeaders(http);

        if (http.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException("Token JWT ausente ou inválido.");

        var sub =
            http.User.FindFirstValue(AuthExtensions.SubClaimType)
            ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? http.User.FindFirstValue("userId")
            ?? http.User.FindFirstValue("usuarioId");

        if (string.IsNullOrWhiteSpace(sub))
            throw new UnauthorizedException("Claim 'sub' ausente no JWT.");

        sub = sub.Trim();

        // Compat: tokens locais antigos com sub numérico.
        if (long.TryParse(sub, out var idFromSub) && idFromSub > 0)
            return idFromSub;

        var usuarios = http.RequestServices.GetRequiredService<IUsuarioRefRepository>();
        var usuario = await usuarios.GetByEmailAsync(sub, cancellationToken);
        if (usuario is null)
        {
            throw new UnauthorizedException(
                $"Usuário com email '{sub}' não encontrado em USUARIO_HB.");
        }

        if (!AtivoFlags.IsAtivo(usuario.Ativo))
            throw new UnauthorizedException("Usuário inativo.");

        return usuario.Id;
    }

    public static string GetPapel(this HttpContext http)
    {
        var auth = http.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;

        if (!auth.Enabled)
            return DevUserExtensions.GetPapelFromHeaders(http);

        if (http.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException("Token JWT ausente ou inválido.");

        var papel =
            http.User.FindFirstValue(AuthExtensions.RoleClaimType)
            ?? http.User.FindFirstValue(ClaimTypes.Role)
            ?? http.User.FindFirstValue("papel");

        if (!string.IsNullOrWhiteSpace(papel))
        {
            papel = papel.Trim().ToUpperInvariant();
            if (papel.StartsWith("ROLE_", StringComparison.Ordinal))
                papel = papel["ROLE_".Length..];

            if (PapelUsuario.IsValid(papel))
                return papel;
        }

        throw new UnauthorizedException("Claim 'role' ausente ou inválida no JWT (use CIDADAO ou GESTOR).");
    }
}

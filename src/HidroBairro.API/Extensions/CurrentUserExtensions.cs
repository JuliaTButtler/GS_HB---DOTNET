using System.Security.Claims;
using HidroBairro.API.Auth;
using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using Microsoft.Extensions.Options;

namespace HidroBairro.API.Extensions;

/// <summary>
/// Resolve usuário/papel: JWT (quando Auth ligado) ou headers DevUser (quando Auth desligado).
/// </summary>
public static class CurrentUserExtensions
{
    public static long GetUsuarioId(this HttpContext http)
    {
        var auth = http.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;

        if (auth.Enabled)
        {
            if (http.User.Identity?.IsAuthenticated != true)
                throw new UnauthorizedException("Token JWT ausente ou inválido.");

            var raw =
                http.User.FindFirstValue(AuthExtensions.SubClaimType)
                ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? http.User.FindFirstValue("userId")
                ?? http.User.FindFirstValue("usuarioId");

            if (long.TryParse(raw, out var id) && id > 0)
                return id;

            throw new UnauthorizedException("Claim 'sub' (usuarioId) ausente ou inválida no JWT.");
        }

        return DevUserExtensions.GetUsuarioIdFromHeaders(http);
    }

    public static string GetPapel(this HttpContext http)
    {
        var auth = http.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;

        if (auth.Enabled)
        {
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

        return DevUserExtensions.GetPapelFromHeaders(http);
    }
}

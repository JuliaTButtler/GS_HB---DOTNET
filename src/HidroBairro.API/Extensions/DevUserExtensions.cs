using HidroBairro.Domain.Models;

namespace HidroBairro.API.Extensions;

/// <summary>
/// Usuário de desenvolvimento (sem JWT). Headers: X-User-Id, X-User-Role.
/// Defaults: 1 / CIDADAO — suficiente para Swagger e Mobile até a etapa JWT.
/// </summary>
public static class DevUserExtensions
{
    public const string UserIdHeader = "X-User-Id";
    public const string UserRoleHeader = "X-User-Role";

    public static long GetUsuarioId(this HttpContext http)
    {
        if (http.Request.Headers.TryGetValue(UserIdHeader, out var raw) &&
            long.TryParse(raw.ToString(), out var id) &&
            id > 0)
        {
            return id;
        }

        return 1;
    }

    public static string GetPapel(this HttpContext http)
    {
        if (http.Request.Headers.TryGetValue(UserRoleHeader, out var raw))
        {
            var papel = raw.ToString().Trim().ToUpperInvariant();
            if (PapelUsuario.IsValid(papel))
                return papel;
        }

        return PapelUsuario.Cidadao;
    }
}

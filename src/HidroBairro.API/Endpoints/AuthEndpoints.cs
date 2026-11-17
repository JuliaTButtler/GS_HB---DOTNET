using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HidroBairro.API.Auth;
using HidroBairro.API.Extensions;
using HidroBairro.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HidroBairro.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
        if (!auth.Enabled || !auth.LocalIssuerEnabled)
            return app;

        var group = app.MapGroup("/api/auth")
            .WithTags("Auth (local / Dev)")
            .AllowAnonymous();

        group.MapPost("/dev-token", (
            [FromBody] DevTokenRequest request,
            IOptions<AuthOptions> options) =>
        {
            if (request.UsuarioId <= 0)
            {
                return Results.BadRequest(new { error = "UsuarioId deve ser > 0." });
            }

            var papel = (request.Papel ?? string.Empty).Trim().ToUpperInvariant();
            if (papel.StartsWith("ROLE_", StringComparison.Ordinal))
                papel = papel["ROLE_".Length..];

            if (!PapelUsuario.IsValid(papel))
            {
                return Results.BadRequest(new
                {
                    error = "Papel inválido. Use CIDADAO ou GESTOR."
                });
            }

            var cfg = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(cfg.ExpirationMinutes);

            // Uma única claim "sub" (string). Duplicar vira array no JWT e o Bearer rejeita como invalid_token.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, request.UsuarioId.ToString()),
                new(AuthExtensions.RoleClaimType, papel)
            };

            var token = new JwtSecurityToken(
                issuer: cfg.Issuer,
                audience: cfg.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                accessToken = jwt,
                tokenType = "Bearer",
                expiresAtUtc = expires,
                usuarioId = request.UsuarioId,
                papel,
                note = "Token local de desenvolvimento. Troque LocalIssuerEnabled=false quando o Java emitir o JWT oficial."
            });
        })
        .WithName("IssueDevToken")
        .WithSummary("Emite JWT local (Dev) até a API Java ser o issuer")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}

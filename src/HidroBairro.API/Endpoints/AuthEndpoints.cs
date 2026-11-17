using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return Results.BadRequest(new { error = "Email válido é obrigatório (vai na claim sub, como no Java)." });
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
            var key = JwtKeyHelper.CreateSigningKey(cfg);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(cfg.ExpirationMinutes);
            var email = request.Email.Trim();

            // Alinhado ao Java: sub=email, role=CIDADAO|GESTOR, sem iss/aud.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, email),
                new(AuthExtensions.RoleClaimType, papel)
            };

            var token = new JwtSecurityToken(
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
                email,
                papel,
                note = "Token local no mesmo formato do Java (sub=email, role). Prefira o token do login Java em integração real."
            });
        })
        .WithName("IssueDevToken")
        .WithSummary("Emite JWT local no formato Java (sub=email, role)")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}

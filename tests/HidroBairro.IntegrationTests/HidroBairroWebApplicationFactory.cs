using HidroBairro.API.Auth;
using HidroBairro.Domain.Models;
using HidroBairro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HidroBairro.IntegrationTests;

public sealed class HidroBairroWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Mesmo secret Base64 do Java (material HMAC após decode).</summary>
    public const string TestAuthKeyBase64 = "61BMUo/4EwnkPt7Ep1hnAjjjdh8LCHJIs5OeZjBc7gQ=";

    private readonly string _databaseName = $"HidroBairroTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext)))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.PostConfigure<AuthOptions>(auth =>
            {
                auth.Enabled = true;
                auth.LocalIssuerEnabled = true;
                auth.Key = TestAuthKeyBase64;
                auth.KeyIsBase64 = true;
                auth.ValidateIssuer = false;
                auth.ValidateAudience = false;
                auth.ExpirationMinutes = 60;
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = JwtKeyHelper.CreateSigningKey(new AuthOptions
                {
                    Key = TestAuthKeyBase64,
                    KeyIsBase64 = true
                });

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = "role",
                    NameClaimType = "sub"
                };
            });
        });
    }

    public async Task EnsureSeedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.Add(new UsuarioRef
            {
                Id = 1,
                Nome = "Carla Souza",
                Email = "carla.souza@email.com",
                Papel = PapelUsuario.Cidadao,
                Ativo = AtivoFlags.Sim
            });
            db.Usuarios.Add(new UsuarioRef
            {
                Id = 12,
                Nome = "Bruno Teixeira",
                Email = "bruno.teixeira@email.com",
                Papel = PapelUsuario.Gestor,
                Ativo = AtivoFlags.Sim
            });
        }

        if (!await db.Categorias.AnyAsync())
        {
            db.Categorias.Add(new CategoriaSolicitacao
            {
                Nome = "Bueiro",
                Descricao = "Bueiro entupido / com defeito",
                Ativo = AtivoFlags.Sim
            });
            db.Categorias.Add(new CategoriaSolicitacao
            {
                Nome = "Boca de lobo",
                Descricao = "Boca de lobo obstruída",
                Ativo = AtivoFlags.Sim
            });
        }

        await db.SaveChangesAsync();
    }
}

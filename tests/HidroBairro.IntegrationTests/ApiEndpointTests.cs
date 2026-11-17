using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HidroBairro.IntegrationTests;

public class ApiEndpointTests : IClassFixture<HidroBairroWebApplicationFactory>
{
    private readonly HidroBairroWebApplicationFactory _factory;

    public ApiEndpointTests(HidroBairroWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DevToken_EmiteJwtLocal()
    {
        await _factory.EnsureSeedAsync();
        var client = _factory.CreateClient();

        var token = await AuthTestHelper.IssueTokenAsync(client, email: "carla.souza@email.com", papel: "CIDADAO");

        Assert.StartsWith("eyJ", token);
        Assert.Equal(2, token.Count(c => c == '.'));
    }

    [Fact]
    public async Task Categorias_SemToken_Retorna401()
    {
        await _factory.EnsureSeedAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Categorias_TokenInvalido_Retorna401()
    {
        await _factory.EnsureSeedAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token.invalido.aqui");

        var response = await client.GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Categorias_ComToken_Retorna200()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/categorias");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Bueiro", body);
    }

    [Fact]
    public async Task CreateSolicitacao_SemIdempotencyKey_Retorna400()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync("/api/solicitacoes", new
        {
            categoriaId = 1,
            titulo = "Bueiro entupido",
            descricao = "Teste integração",
            latitude = -23.65m,
            longitude = -46.75m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Idempotency-Key", body);
    }

    [Fact]
    public async Task CreateSolicitacao_ComToken_Retorna201()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var key = $"it-create-{Guid.NewGuid():N}";

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/solicitacoes");
        request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        request.Content = JsonContent.Create(new
        {
            categoriaId = 1,
            titulo = "Bueiro entupido",
            descricao = "Teste integração create",
            latitude = -23.65m,
            longitude = -46.75m
        });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.True(doc.RootElement.TryGetProperty("id", out var idProp));
        Assert.True(idProp.GetInt64() > 0);
        Assert.Equal(key, doc.RootElement.GetProperty("idempotencyKey").GetString());
    }

    [Fact]
    public async Task CreateSolicitacao_MesmaIdempotencyKey_NaoDuplica()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var key = $"it-idem-{Guid.NewGuid():N}";
        var payload = new
        {
            categoriaId = 1,
            titulo = "Boca de lobo",
            descricao = "Teste idempotência",
            latitude = -23.66m,
            longitude = -46.76m
        };

        async Task<HttpResponseMessage> PostAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/solicitacoes");
            request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
            request.Content = JsonContent.Create(payload);
            return await client.SendAsync(request);
        }

        var first = await PostAsync();
        var second = await PostAsync();

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstBody = await first.Content.ReadAsStringAsync();
        var secondBody = await second.Content.ReadAsStringAsync();
        using var firstDoc = JsonDocument.Parse(firstBody);
        using var secondDoc = JsonDocument.Parse(secondBody);

        Assert.Equal(
            firstDoc.RootElement.GetProperty("id").GetInt64(),
            secondDoc.RootElement.GetProperty("id").GetInt64());
    }

    [Fact]
    public async Task ListSolicitacoes_ComToken_Retorna200()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/solicitacoes?somenteDoUsuario=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HidroBairro.IntegrationTests;

public class SolicitacaoLifecycleTests : IClassFixture<HidroBairroWebApplicationFactory>
{
    private readonly HidroBairroWebApplicationFactory _factory;

    public SolicitacaoLifecycleTests(HidroBairroWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_Dono_Retorna200()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(client, "it-get");

        var response = await client.GetAsync($"/api/solicitacoes/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.Equal(id, doc.RootElement.GetProperty("id").GetInt64());
    }

    [Fact]
    public async Task GetById_OutroCidadao_Retorna403()
    {
        await _factory.EnsureSeedAsync();
        var dono = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(dono, "it-forbid-get");

        var outro = await AuthTestHelper.CreateAuthenticatedClientAsync(
            _factory,
            email: "joao.silva@email.com",
            papel: "CIDADAO");

        var response = await outro.GetAsync($"/api/solicitacoes/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Historico_AposCreate_RetornaAbertura()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(client, "it-hist");

        var response = await client.GetAsync($"/api/solicitacoes/{id}/historico");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("RECEBIDA", body);
        Assert.Contains("Abertura", body);
    }

    [Fact]
    public async Task Update_Recebida_Retorna200()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(client, "it-upd");

        var response = await client.PutAsJsonAsync($"/api/solicitacoes/{id}", new
        {
            titulo = "Título atualizado",
            descricao = "Descrição atualizada na integração"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.Equal("Título atualizado", doc.RootElement.GetProperty("titulo").GetString());
    }

    [Fact]
    public async Task Cancelar_Cidadao_Retorna200Cancelada()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(client, "it-cancel");

        var response = await client.PostAsync(
            $"/api/solicitacoes/{id}/cancelar?observacao=Desisti",
            null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.Equal("CANCELADA", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ChangeStatus_Gestor_EmVistoria()
    {
        await _factory.EnsureSeedAsync();
        var cidadao = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(cidadao, "it-status");

        var gestor = await AuthTestHelper.CreateAuthenticatedClientAsync(
            _factory,
            email: "bruno.teixeira@email.com",
            papel: "GESTOR");

        var response = await gestor.PatchAsJsonAsync($"/api/solicitacoes/{id}/status", new
        {
            statusNovo = "EM_VISTORIA",
            observacao = "Equipe a caminho"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.Equal("EM_VISTORIA", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ChangeStatus_CidadaoTentaEmVistoria_Retorna403()
    {
        await _factory.EnsureSeedAsync();
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(client, "it-cid-status");

        var response = await client.PatchAsJsonAsync($"/api/solicitacoes/{id}/status", new
        {
            statusNovo = "EM_VISTORIA"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_Gestor_FilaCompleta_Retorna200()
    {
        await _factory.EnsureSeedAsync();
        var cidadao = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        await CriarSolicitacaoAsync(cidadao, "it-fila");

        var gestor = await AuthTestHelper.CreateAuthenticatedClientAsync(
            _factory,
            email: "bruno.teixeira@email.com",
            papel: "GESTOR");

        var response = await gestor.GetAsync("/api/solicitacoes?somenteDoUsuario=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("items", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Update_OutroCidadao_Retorna403()
    {
        await _factory.EnsureSeedAsync();
        var dono = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);
        var id = await CriarSolicitacaoAsync(dono, "it-upd-forbid");

        var outro = await AuthTestHelper.CreateAuthenticatedClientAsync(
            _factory,
            email: "joao.silva@email.com",
            papel: "CIDADAO");

        var response = await outro.PutAsJsonAsync($"/api/solicitacoes/{id}", new
        {
            titulo = "Hack",
            descricao = "Não deveria passar"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<long> CriarSolicitacaoAsync(HttpClient client, string prefix)
    {
        var key = $"{prefix}-{Guid.NewGuid():N}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/solicitacoes");
        request.Headers.TryAddWithoutValidation("Idempotency-Key", key);
        request.Content = JsonContent.Create(new
        {
            categoriaId = 1,
            titulo = $"Chamado {prefix}",
            descricao = $"Integração {prefix}",
            latitude = -23.65m,
            longitude = -46.75m
        });

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("id").GetInt64();
    }
}

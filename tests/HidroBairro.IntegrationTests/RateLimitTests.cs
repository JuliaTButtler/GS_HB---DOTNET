using System.Net;
using System.Net.Http.Headers;

namespace HidroBairro.IntegrationTests;

public class RateLimitTests : IClassFixture<RateLimitWebApplicationFactory>
{
    private readonly RateLimitWebApplicationFactory _factory;

    public RateLimitTests(RateLimitWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Categorias_ExcedeLimite_Retorna429()
    {
        await _factory.EnsureSeedAsync();
        var client = _factory.CreateClient();
        var token = await AuthTestHelper.IssueTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage? last = null;
        for (var i = 0; i < 8; i++)
            last = await client.GetAsync("/api/categorias");

        Assert.NotNull(last);
        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);

        var body = await last.Content.ReadAsStringAsync();
        Assert.Contains("429", body);
        Assert.Contains("Limite", body, StringComparison.OrdinalIgnoreCase);
    }
}

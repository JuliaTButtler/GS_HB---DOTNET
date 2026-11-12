using Microsoft.AspNetCore.Mvc.Testing;

namespace HidroBairro.IntegrationTests;

public class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Root_ReturnsOk()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("HidroBairro.API", body);
    }
}

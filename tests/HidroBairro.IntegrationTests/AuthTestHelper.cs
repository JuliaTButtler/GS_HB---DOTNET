using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HidroBairro.IntegrationTests;

internal static class AuthTestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> IssueTokenAsync(
        HttpClient client,
        long usuarioId = 1,
        string papel = "CIDADAO")
    {
        var response = await client.PostAsJsonAsync("/api/auth/dev-token", new
        {
            usuarioId,
            papel
        });

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var token = doc.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token!;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        HidroBairroWebApplicationFactory factory,
        long usuarioId = 1,
        string papel = "CIDADAO")
    {
        var client = factory.CreateClient();
        var token = await IssueTokenAsync(client, usuarioId, papel);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

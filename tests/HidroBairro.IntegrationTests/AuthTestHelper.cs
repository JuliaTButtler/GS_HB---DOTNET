using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HidroBairro.IntegrationTests;

internal static class AuthTestHelper
{
    public static async Task<string> IssueTokenAsync(
        HttpClient client,
        string email = "carla.souza@email.com",
        string papel = "CIDADAO")
    {
        var response = await client.PostAsJsonAsync("/api/auth/dev-token", new
        {
            email,
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
        string email = "carla.souza@email.com",
        string papel = "CIDADAO")
    {
        var client = factory.CreateClient();
        var token = await IssueTokenAsync(client, email, papel);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

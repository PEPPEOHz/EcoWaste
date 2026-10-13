using System.Net;

namespace EcoWaste.Tests;

/// <summary>
/// Garante que o endpoint /health (usado pelo Docker e pelo pipeline de deploy) responde 200.
/// </summary>
public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsHttpStatusCode200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }
}

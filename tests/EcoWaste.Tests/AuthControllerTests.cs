using System.Net;
using System.Net.Http.Json;
using EcoWaste.Api.ViewModels;

namespace EcoWaste.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/Auth/login";
        var body = new LoginViewModel { Username = "admin", Password = "Admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync(request, body);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var body = new LoginViewModel { Username = "admin", Password = "errada" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", body);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoint_WithoutToken_Returns401()
    {
        var body = new CollectionPointCreateViewModel
        {
            Name = "Teste", Address = "Rua X", CapacityKg = 100
        };
        var response = await _client.PostAsJsonAsync("/api/CollectionPoints", body);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoint_WithToken_Returns201()
    {
        var token = await TestAuth.GetAdminTokenAsync(_client);
        TestAuth.Authorize(_client, token);

        var body = new CollectionPointCreateViewModel
        {
            Name = "Ecoponto Teste", Address = "Rua Y, 10", CapacityKg = 300, AlertThresholdPercent = 80
        };
        var response = await _client.PostAsJsonAsync("/api/CollectionPoints", body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}

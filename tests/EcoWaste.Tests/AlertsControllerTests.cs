namespace EcoWaste.Tests;

public class AlertsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AlertsControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/Alerts";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
    }
}

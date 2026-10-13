namespace EcoWaste.Tests;

public class WasteRecordsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WasteRecordsControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/WasteRecords";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
    }

    [Fact]
    public async Task GetReport_ReturnsHttpStatusCode200()
    {
        var response = await _client.GetAsync("/api/WasteRecords/report");
        response.EnsureSuccessStatusCode();
    }
}

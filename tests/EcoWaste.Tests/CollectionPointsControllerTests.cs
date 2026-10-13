namespace EcoWaste.Tests;

public class CollectionPointsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CollectionPointsControllerTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/CollectionPoints";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPaged_ReturnsHttpStatusCode200()
    {
        var response = await _client.GetAsync("/api/CollectionPoints?pageNumber=1&pageSize=5");
        response.EnsureSuccessStatusCode();
    }
}

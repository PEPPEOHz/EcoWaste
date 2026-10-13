using System.Net.Http.Headers;
using System.Net.Http.Json;
using EcoWaste.Api.ViewModels;

namespace EcoWaste.Tests;

internal static class TestAuth
{
    /// <summary>Faz login com o usuário admin semeado e devolve o token JWT.</summary>
    public static async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginViewModel { Username = "admin", Password = "Admin@123" });
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();
        return token!.AccessToken;
    }

    public static void Authorize(HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}

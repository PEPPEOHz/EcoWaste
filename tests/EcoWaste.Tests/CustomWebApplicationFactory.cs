using EcoWaste.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcoWaste.Tests;

/// <summary>
/// Fábrica de aplicação para testes de integração. Substitui o provedor Oracle
/// por um banco InMemory, permitindo executar os testes sem infraestrutura externa.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove o DbContext configurado para Oracle.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Registra um DbContext InMemory para os testes.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("EcoWasteTestDb"));

            // Garante que o banco seja criado e populado.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}

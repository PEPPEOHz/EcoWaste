using EcoWaste.Api.Models;
using EcoWaste.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Aplica migrações pendentes automaticamente (somente provedores relacionais).
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                Role = "Admin"
            });
            db.Users.Add(new AppUser
            {
                Username = "operador",
                PasswordHash = PasswordHasher.Hash("Operador@123"),
                Role = "Operator"
            });
        }

        if (!await db.CollectionPoints.AnyAsync())
        {
            db.CollectionPoints.AddRange(
                new CollectionPoint
                {
                    Name = "Ecoponto Centro",
                    Address = "Av. Paulista, 1000 - São Paulo/SP",
                    Latitude = -23.561, Longitude = -46.656,
                    CapacityKg = 500, AlertThresholdPercent = 80
                },
                new CollectionPoint
                {
                    Name = "Estação Zona Sul",
                    Address = "Rua das Acácias, 250 - São Paulo/SP",
                    Latitude = -23.640, Longitude = -46.700,
                    CapacityKg = 800, AlertThresholdPercent = 75
                });
        }

        await db.SaveChangesAsync();
    }
}

using EcoWaste.Api.Models;

namespace EcoWaste.Api.ViewModels;

public class CollectionPointCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CapacityKg { get; set; }
    public double AlertThresholdPercent { get; set; } = 80;
}

public class CollectionPointUpdateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CapacityKg { get; set; }
    public double AlertThresholdPercent { get; set; }
    public bool Active { get; set; }
}

public class CollectionPointViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double CapacityKg { get; set; }
    public double CurrentLoadKg { get; set; }
    public double OccupancyPercent { get; set; }
    public double AlertThresholdPercent { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CollectionPointViewModel FromModel(CollectionPoint p, double currentLoadKg)
        => new()
        {
            Id = p.Id,
            Name = p.Name,
            Address = p.Address,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            CapacityKg = p.CapacityKg,
            CurrentLoadKg = currentLoadKg,
            OccupancyPercent = p.CapacityKg <= 0 ? 0 : Math.Round(currentLoadKg / p.CapacityKg * 100, 2),
            AlertThresholdPercent = p.AlertThresholdPercent,
            Active = p.Active,
            CreatedAt = p.CreatedAt
        };
}

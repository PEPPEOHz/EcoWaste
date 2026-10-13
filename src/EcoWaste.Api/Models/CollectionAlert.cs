using System.ComponentModel.DataAnnotations;

namespace EcoWaste.Api.Models;

/// <summary>
/// Alerta gerado automaticamente quando um ponto atinge o limite de ocupação.
/// </summary>
public class CollectionAlert
{
    public int Id { get; set; }

    public int CollectionPointId { get; set; }
    public CollectionPoint? CollectionPoint { get; set; }

    public double CurrentLoadKg { get; set; }
    public double OccupancyPercent { get; set; }

    [MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace EcoWaste.Api.Models;

/// <summary>
/// Registro de descarte/coleta de resíduos em um ponto de coleta.
/// </summary>
public class WasteRecord
{
    public int Id { get; set; }

    public int CollectionPointId { get; set; }
    public CollectionPoint? CollectionPoint { get; set; }

    public WasteType Type { get; set; }

    /// <summary>Quantidade depositada, em quilogramas.</summary>
    public double WeightKg { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    /// <summary>True quando o material já foi recolhido pela coleta.</summary>
    public bool Collected { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace EcoWaste.Api.Models;

/// <summary>
/// Ponto de coleta seletiva (ex.: ecoponto, contêiner, estação de reciclagem).
/// </summary>
public class CollectionPoint
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>Capacidade total em quilogramas.</summary>
    public double CapacityKg { get; set; }

    /// <summary>Percentual (0-100) a partir do qual um alerta de coleta é gerado.</summary>
    public double AlertThresholdPercent { get; set; } = 80;

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WasteRecord> WasteRecords { get; set; } = new List<WasteRecord>();
}

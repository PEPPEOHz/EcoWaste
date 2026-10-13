using EcoWaste.Api.Models;

namespace EcoWaste.Api.ViewModels;

public class WasteRecordCreateViewModel
{
    public int CollectionPointId { get; set; }
    public WasteType Type { get; set; }
    public double WeightKg { get; set; }
    public string? Description { get; set; }
}

public class WasteRecordViewModel
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public WasteType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public double WeightKg { get; set; }
    public string? Description { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool Collected { get; set; }

    public static WasteRecordViewModel FromModel(WasteRecord r)
        => new()
        {
            Id = r.Id,
            CollectionPointId = r.CollectionPointId,
            CollectionPointName = r.CollectionPoint?.Name ?? string.Empty,
            Type = r.Type,
            TypeName = r.Type.ToString(),
            WeightKg = r.WeightKg,
            Description = r.Description,
            RecordedAt = r.RecordedAt,
            Collected = r.Collected
        };
}

/// <summary>Resposta enriquecida ao registrar resíduo: indica se um alerta foi disparado.</summary>
public class WasteRecordResultViewModel
{
    public WasteRecordViewModel Record { get; set; } = new();
    public bool AlertTriggered { get; set; }
    public CollectionAlertViewModel? Alert { get; set; }
    public double OccupancyPercent { get; set; }
}

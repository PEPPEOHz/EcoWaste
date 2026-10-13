using EcoWaste.Api.Models;

namespace EcoWaste.Api.ViewModels;

public class CollectionAlertViewModel
{
    public int Id { get; set; }
    public int CollectionPointId { get; set; }
    public string CollectionPointName { get; set; } = string.Empty;
    public double CurrentLoadKg { get; set; }
    public double OccupancyPercent { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public static CollectionAlertViewModel FromModel(CollectionAlert a)
        => new()
        {
            Id = a.Id,
            CollectionPointId = a.CollectionPointId,
            CollectionPointName = a.CollectionPoint?.Name ?? string.Empty,
            CurrentLoadKg = a.CurrentLoadKg,
            OccupancyPercent = a.OccupancyPercent,
            Message = a.Message,
            CreatedAt = a.CreatedAt,
            Resolved = a.Resolved,
            ResolvedAt = a.ResolvedAt
        };
}

/// <summary>Linha do relatório agregado de reciclagem por tipo de resíduo.</summary>
public class RecyclingReportItemViewModel
{
    public WasteType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public double TotalWeightKg { get; set; }
    public int RecordCount { get; set; }
    public double SharePercent { get; set; }
}

public class RecyclingReportViewModel
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public double TotalWeightKg { get; set; }
    public IEnumerable<RecyclingReportItemViewModel> ByType { get; set; }
        = Enumerable.Empty<RecyclingReportItemViewModel>();
}

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TokenViewModel
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresInSeconds { get; set; }
    public string Role { get; set; } = string.Empty;
}

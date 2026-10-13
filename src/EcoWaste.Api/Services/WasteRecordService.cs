using EcoWaste.Api.Data;
using EcoWaste.Api.Exceptions;
using EcoWaste.Api.Models;
using EcoWaste.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Services;

public interface IWasteRecordService
{
    Task<PagedResult<WasteRecordViewModel>> GetPagedAsync(
        PaginationQuery pagination, int? collectionPointId, WasteType? type, bool? collected, CancellationToken ct);
    Task<WasteRecordResultViewModel> RegisterAsync(WasteRecordCreateViewModel vm, CancellationToken ct);
    Task<RecyclingReportViewModel> GetReportAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct);
}

public class WasteRecordService : IWasteRecordService
{
    private readonly AppDbContext _db;

    public WasteRecordService(AppDbContext db) => _db = db;

    public async Task<PagedResult<WasteRecordViewModel>> GetPagedAsync(
        PaginationQuery pagination, int? collectionPointId, WasteType? type, bool? collected, CancellationToken ct)
    {
        var query = _db.WasteRecords
            .AsNoTracking()
            .Include(w => w.CollectionPoint)
            .AsQueryable();

        if (collectionPointId.HasValue) query = query.Where(w => w.CollectionPointId == collectionPointId);
        if (type.HasValue) query = query.Where(w => w.Type == type);
        if (collected.HasValue) query = query.Where(w => w.Collected == collected);

        query = query.OrderByDescending(w => w.RecordedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<WasteRecordViewModel>
        {
            Items = items.Select(WasteRecordViewModel.FromModel),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = total
        };
    }

    public async Task<WasteRecordResultViewModel> RegisterAsync(WasteRecordCreateViewModel vm, CancellationToken ct)
    {
        if (vm.WeightKg <= 0)
            throw new BusinessRuleException("O peso deve ser maior que zero.");

        var point = await _db.CollectionPoints.FirstOrDefaultAsync(p => p.Id == vm.CollectionPointId, ct)
            ?? throw new NotFoundException($"Ponto de coleta {vm.CollectionPointId} não encontrado.");

        if (!point.Active)
            throw new BusinessRuleException("Não é possível registrar resíduo em um ponto inativo.");

        var record = new WasteRecord
        {
            CollectionPointId = point.Id,
            Type = vm.Type,
            WeightKg = vm.WeightKg,
            Description = vm.Description?.Trim()
        };
        _db.WasteRecords.Add(record);
        await _db.SaveChangesAsync(ct);

        // Recalcula ocupação e dispara alerta automático se ultrapassar o limite.
        var currentLoad = await _db.WasteRecords
            .Where(w => w.CollectionPointId == point.Id && !w.Collected)
            .SumAsync(w => (double?)w.WeightKg, ct) ?? 0;

        var occupancy = point.CapacityKg <= 0 ? 0 : Math.Round(currentLoad / point.CapacityKg * 100, 2);

        CollectionAlert? alert = null;
        if (occupancy >= point.AlertThresholdPercent)
        {
            var hasOpenAlert = await _db.CollectionAlerts
                .AnyAsync(a => a.CollectionPointId == point.Id && !a.Resolved, ct);

            if (!hasOpenAlert)
            {
                alert = new CollectionAlert
                {
                    CollectionPointId = point.Id,
                    CurrentLoadKg = currentLoad,
                    OccupancyPercent = occupancy,
                    Message = $"Ponto '{point.Name}' atingiu {occupancy}% da capacidade. Coleta recomendada."
                };
                _db.CollectionAlerts.Add(alert);
                await _db.SaveChangesAsync(ct);
            }
        }

        record.CollectionPoint = point;
        return new WasteRecordResultViewModel
        {
            Record = WasteRecordViewModel.FromModel(record),
            AlertTriggered = alert != null,
            Alert = alert is null ? null : CollectionAlertViewModel.FromModel(alert),
            OccupancyPercent = occupancy
        };
    }

    public async Task<RecyclingReportViewModel> GetReportAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct)
    {
        var from = fromUtc ?? DateTime.UtcNow.AddDays(-30);
        var to = toUtc ?? DateTime.UtcNow;

        if (from > to)
            throw new BusinessRuleException("A data inicial não pode ser maior que a data final.");

        var grouped = await _db.WasteRecords
            .AsNoTracking()
            .Where(w => w.RecordedAt >= from && w.RecordedAt <= to)
            .GroupBy(w => w.Type)
            .Select(g => new
            {
                Type = g.Key,
                TotalWeightKg = g.Sum(x => x.WeightKg),
                RecordCount = g.Count()
            })
            .ToListAsync(ct);

        var total = grouped.Sum(g => g.TotalWeightKg);

        return new RecyclingReportViewModel
        {
            FromUtc = from,
            ToUtc = to,
            TotalWeightKg = Math.Round(total, 2),
            ByType = grouped
                .OrderByDescending(g => g.TotalWeightKg)
                .Select(g => new RecyclingReportItemViewModel
                {
                    Type = g.Type,
                    TypeName = g.Type.ToString(),
                    TotalWeightKg = Math.Round(g.TotalWeightKg, 2),
                    RecordCount = g.RecordCount,
                    SharePercent = total <= 0 ? 0 : Math.Round(g.TotalWeightKg / total * 100, 2)
                })
                .ToList()
        };
    }
}

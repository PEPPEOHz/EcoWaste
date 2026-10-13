using EcoWaste.Api.Data;
using EcoWaste.Api.Exceptions;
using EcoWaste.Api.Models;
using EcoWaste.Api.Repositories;
using EcoWaste.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Services;

public interface ICollectionPointService
{
    Task<PagedResult<CollectionPointViewModel>> GetPagedAsync(PaginationQuery pagination, bool? activeOnly, CancellationToken ct);
    Task<CollectionPointViewModel> GetByIdAsync(int id, CancellationToken ct);
    Task<CollectionPointViewModel> CreateAsync(CollectionPointCreateViewModel vm, CancellationToken ct);
    Task<CollectionPointViewModel> UpdateAsync(int id, CollectionPointUpdateViewModel vm, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

public class CollectionPointService : ICollectionPointService
{
    private readonly AppDbContext _db;

    public CollectionPointService(AppDbContext db) => _db = db;

    public async Task<PagedResult<CollectionPointViewModel>> GetPagedAsync(
        PaginationQuery pagination, bool? activeOnly, CancellationToken ct)
    {
        var query = _db.CollectionPoints.AsNoTracking().AsQueryable();
        if (activeOnly == true) query = query.Where(p => p.Active);

        // Projeção com carga atual calculada no banco (subconsulta agregada).
        var projected = query
            .OrderBy(p => p.Name)
            .Select(p => new CollectionPointViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                CapacityKg = p.CapacityKg,
                CurrentLoadKg = p.WasteRecords.Where(w => !w.Collected).Sum(w => (double?)w.WeightKg) ?? 0,
                AlertThresholdPercent = p.AlertThresholdPercent,
                Active = p.Active,
                CreatedAt = p.CreatedAt
            });

        var total = await projected.CountAsync(ct);
        var items = await projected
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        foreach (var i in items)
            i.OccupancyPercent = i.CapacityKg <= 0 ? 0 : Math.Round(i.CurrentLoadKg / i.CapacityKg * 100, 2);

        return new PagedResult<CollectionPointViewModel>
        {
            Items = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = total
        };
    }

    public async Task<CollectionPointViewModel> GetByIdAsync(int id, CancellationToken ct)
    {
        var point = await _db.CollectionPoints.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Ponto de coleta {id} não encontrado.");

        var load = await CurrentLoadAsync(id, ct);
        return CollectionPointViewModel.FromModel(point, load);
    }

    public async Task<CollectionPointViewModel> CreateAsync(CollectionPointCreateViewModel vm, CancellationToken ct)
    {
        if (vm.CapacityKg <= 0)
            throw new BusinessRuleException("A capacidade deve ser maior que zero.");

        var point = new CollectionPoint
        {
            Name = vm.Name.Trim(),
            Address = vm.Address.Trim(),
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            CapacityKg = vm.CapacityKg,
            AlertThresholdPercent = vm.AlertThresholdPercent
        };

        _db.CollectionPoints.Add(point);
        await _db.SaveChangesAsync(ct);
        return CollectionPointViewModel.FromModel(point, 0);
    }

    public async Task<CollectionPointViewModel> UpdateAsync(int id, CollectionPointUpdateViewModel vm, CancellationToken ct)
    {
        var point = await _db.CollectionPoints.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Ponto de coleta {id} não encontrado.");

        if (vm.CapacityKg <= 0)
            throw new BusinessRuleException("A capacidade deve ser maior que zero.");

        point.Name = vm.Name.Trim();
        point.Address = vm.Address.Trim();
        point.Latitude = vm.Latitude;
        point.Longitude = vm.Longitude;
        point.CapacityKg = vm.CapacityKg;
        point.AlertThresholdPercent = vm.AlertThresholdPercent;
        point.Active = vm.Active;

        await _db.SaveChangesAsync(ct);

        var load = await CurrentLoadAsync(id, ct);
        return CollectionPointViewModel.FromModel(point, load);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var point = await _db.CollectionPoints.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Ponto de coleta {id} não encontrado.");

        _db.CollectionPoints.Remove(point);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<double> CurrentLoadAsync(int pointId, CancellationToken ct)
    {
        var sum = await _db.WasteRecords
            .Where(w => w.CollectionPointId == pointId && !w.Collected)
            .SumAsync(w => (double?)w.WeightKg, ct);
        return sum ?? 0;
    }
}

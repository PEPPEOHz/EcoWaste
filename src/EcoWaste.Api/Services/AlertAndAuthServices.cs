using EcoWaste.Api.Data;
using EcoWaste.Api.Exceptions;
using EcoWaste.Api.Models;
using EcoWaste.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Services;

public interface IAlertService
{
    Task<PagedResult<CollectionAlertViewModel>> GetPagedAsync(
        PaginationQuery pagination, bool? resolved, CancellationToken ct);
    Task<CollectionAlertViewModel> ResolveAsync(int id, CancellationToken ct);
}

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;

    public AlertService(AppDbContext db) => _db = db;

    public async Task<PagedResult<CollectionAlertViewModel>> GetPagedAsync(
        PaginationQuery pagination, bool? resolved, CancellationToken ct)
    {
        var query = _db.CollectionAlerts
            .AsNoTracking()
            .Include(a => a.CollectionPoint)
            .AsQueryable();

        if (resolved.HasValue) query = query.Where(a => a.Resolved == resolved);

        query = query.OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CollectionAlertViewModel>
        {
            Items = items.Select(CollectionAlertViewModel.FromModel),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = total
        };
    }

    public async Task<CollectionAlertViewModel> ResolveAsync(int id, CancellationToken ct)
    {
        var alert = await _db.CollectionAlerts
            .Include(a => a.CollectionPoint)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException($"Alerta {id} não encontrado.");

        if (alert.Resolved)
            throw new BusinessRuleException("Este alerta já está resolvido.");

        alert.Resolved = true;
        alert.ResolvedAt = DateTime.UtcNow;

        // Ao resolver, marca os resíduos pendentes do ponto como coletados.
        var pending = await _db.WasteRecords
            .Where(w => w.CollectionPointId == alert.CollectionPointId && !w.Collected)
            .ToListAsync(ct);
        foreach (var w in pending) w.Collected = true;

        await _db.SaveChangesAsync(ct);
        return CollectionAlertViewModel.FromModel(alert);
    }
}

public interface IAuthService
{
    Task<TokenViewModel> LoginAsync(LoginViewModel vm, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<TokenViewModel> LoginAsync(LoginViewModel vm, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == vm.Username, ct);
        if (user is null || !PasswordHasher.Verify(vm.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        var (token, expires) = _tokenService.GenerateToken(user);
        return new TokenViewModel
        {
            AccessToken = token,
            ExpiresInSeconds = expires,
            Role = user.Role
        };
    }
}

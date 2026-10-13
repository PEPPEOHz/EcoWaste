using EcoWaste.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Repositories;

/// <summary>
/// Extensão que aplica paginação eficiente no banco (Skip/Take + Count),
/// trazendo apenas a página solicitada em vez de carregar a tabela inteira.
/// </summary>
public static class QueryableExtensions
{
    public static async Task<PagedResult<TOut>> ToPagedResultAsync<TIn, TOut>(
        this IQueryable<TIn> query,
        PaginationQuery pagination,
        Func<TIn, TOut> projection,
        CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<TOut>
        {
            Items = items.Select(projection),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = total
        };
    }
}

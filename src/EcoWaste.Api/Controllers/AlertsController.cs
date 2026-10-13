using EcoWaste.Api.Services;
using EcoWaste.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoWaste.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertsController(IAlertService service) => _service = service;

    /// <summary>Lista alertas de coleta com paginação. Filtro opcional por status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CollectionAlertViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CollectionAlertViewModel>>> GetAll(
        [FromQuery] PaginationQuery pagination,
        [FromQuery] bool? resolved,
        CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(pagination, resolved, ct);
        return Ok(result);
    }

    /// <summary>
    /// Resolve um alerta (registra coleta realizada) — endpoint crítico, requer autenticação.
    /// Marca os resíduos pendentes do ponto como coletados.
    /// </summary>
    [HttpPatch("{id:int}/resolve")]
    [Authorize]
    [ProducesResponseType(typeof(CollectionAlertViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionAlertViewModel>> Resolve(int id, CancellationToken ct)
    {
        var result = await _service.ResolveAsync(id, ct);
        return Ok(result);
    }
}

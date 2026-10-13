using EcoWaste.Api.Models;
using EcoWaste.Api.Services;
using EcoWaste.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoWaste.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WasteRecordsController : ControllerBase
{
    private readonly IWasteRecordService _service;

    public WasteRecordsController(IWasteRecordService service) => _service = service;

    /// <summary>Lista registros de resíduos com paginação e filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WasteRecordViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WasteRecordViewModel>>> GetAll(
        [FromQuery] PaginationQuery pagination,
        [FromQuery] int? collectionPointId,
        [FromQuery] WasteType? type,
        [FromQuery] bool? collected,
        CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(pagination, collectionPointId, type, collected, ct);
        return Ok(result);
    }

    /// <summary>
    /// Registra o descarte de resíduo em um ponto. Dispara automaticamente um alerta
    /// de coleta caso o ponto atinja o limite de ocupação configurado.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(WasteRecordResultViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<WasteRecordResultViewModel>> Register(
        [FromBody] WasteRecordCreateViewModel vm, CancellationToken ct)
    {
        var result = await _service.RegisterAsync(vm, ct);
        return CreatedAtAction(nameof(GetAll), new { id = result.Record.Id }, result);
    }

    /// <summary>
    /// Relatório agregado de reciclagem por tipo de resíduo dentro de um período.
    /// (Otimização: agregação executada diretamente no banco via GROUP BY.)
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(RecyclingReportViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RecyclingReportViewModel>> GetReport(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct)
    {
        var report = await _service.GetReportAsync(fromUtc, toUtc, ct);
        return Ok(report);
    }
}

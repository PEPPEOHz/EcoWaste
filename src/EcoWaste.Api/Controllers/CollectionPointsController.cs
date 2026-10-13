using EcoWaste.Api.Services;
using EcoWaste.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoWaste.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CollectionPointsController : ControllerBase
{
    private readonly ICollectionPointService _service;

    public CollectionPointsController(ICollectionPointService service) => _service = service;

    /// <summary>Lista pontos de coleta com paginação e ocupação atual.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CollectionPointViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CollectionPointViewModel>>> GetAll(
        [FromQuery] PaginationQuery pagination,
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
    {
        var result = await _service.GetPagedAsync(pagination, activeOnly, ct);
        return Ok(result);
    }

    /// <summary>Obtém um ponto de coleta pelo identificador.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CollectionPointViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionPointViewModel>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Cria um novo ponto de coleta (requer autenticação).</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CollectionPointViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionPointViewModel>> Create(
        [FromBody] CollectionPointCreateViewModel vm, CancellationToken ct)
    {
        var created = await _service.CreateAsync(vm, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um ponto de coleta existente (requer autenticação).</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(CollectionPointViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionPointViewModel>> Update(
        int id, [FromBody] CollectionPointUpdateViewModel vm, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, vm, ct);
        return Ok(updated);
    }

    /// <summary>Remove um ponto de coleta (somente Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}

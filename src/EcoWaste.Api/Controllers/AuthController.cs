using EcoWaste.Api.Services;
using EcoWaste.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcoWaste.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Autentica um usuário e retorna um token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenViewModel>> Login(
        [FromBody] LoginViewModel vm, CancellationToken ct)
    {
        var token = await _authService.LoginAsync(vm, ct);
        return Ok(token);
    }
}

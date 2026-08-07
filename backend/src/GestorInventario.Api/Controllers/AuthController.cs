using GestorInventario.Api.Authentication;
using GestorInventario.Api.Contracts;
using GestorInventario.Api.Contracts.Requests;
using GestorInventario.Api.Contracts.Responses;
using GestorInventario.Application.Usuarios.Queries.ObtenerSesion;
using GestorInventario.Application.Usuarios.Queries.SugerirNombreUsuario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorInventario.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ApiSessionService _sessionService;

    public AuthController(ISender sender, ApiSessionService sessionService)
    {
        _sender = sender;
        _sessionService = sessionService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SesionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SesionResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(request.ToCommand(), cancellationToken);
        await _sessionService.SignInAsync(HttpContext, usuario, cancellationToken);

        return Ok(new SesionResponse(usuario.ToResponse()));
    }

    [AllowAnonymous]
    [HttpPost("registro")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Registrar(
        [FromBody] RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(request.ToCommand(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, usuario.ToResponse());
    }

    [AllowAnonymous]
    [HttpGet("sugerencia-usuario")]
    [ProducesResponseType(
        typeof(SugerenciaUsuarioResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SugerenciaUsuarioResponse>> SugerirUsuario(
        [FromQuery] string nombre,
        [FromQuery] string apellidos,
        CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(
            new SugerirNombreUsuarioQuery(nombre, apellidos),
            cancellationToken);

        return Ok(new SugerenciaUsuarioResponse(usuario));
    }

    [Authorize(Policy = AuthPolicies.Productos)]
    [HttpGet("sesion")]
    [ProducesResponseType(typeof(SesionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SesionResponse>> ObtenerSesion(
        CancellationToken cancellationToken)
    {
        var usuarioId = _sessionService.GetUsuarioId(User);
        var usuario = await _sender.Send(
            new ObtenerSesionQuery(usuarioId),
            cancellationToken);

        return Ok(new SesionResponse(usuario.ToResponse()));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await _sessionService.SignOutAsync(HttpContext);
        return NoContent();
    }
}

using GestorInventario.Api.Authentication;
using GestorInventario.Api.Contracts;
using GestorInventario.Api.Contracts.Responses;
using GestorInventario.Application.Categorias.Queries.ListarCategorias;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorInventario.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
[Authorize(Policy = AuthPolicies.Productos)]
public sealed class CategoriasController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CategoriaResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoriaResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        var categorias = await _sender.Send(
            new ListarCategoriasQuery(),
            cancellationToken);

        return Ok(categorias.Select(categoria => categoria.ToResponse()).ToArray());
    }
}

using GestorInventario.Api.Contracts;
using GestorInventario.Api.Contracts.Requests;
using GestorInventario.Api.Contracts.Responses;
using GestorInventario.Api.Authentication;
using GestorInventario.Application.Productos.Commands.EliminarProducto;
using GestorInventario.Application.Productos.Queries.ListarProductos;
using GestorInventario.Application.Productos.Queries.ListarProductosStockBajo;
using GestorInventario.Application.Productos.Queries.ObtenerProductoPorId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorInventario.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Produces("application/json")]
[Authorize(Policy = AuthPolicies.Productos)]
public sealed class ProductosController : ControllerBase
{
    private readonly ISender _sender;

    public ProductosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProductoResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductoResponse>>> Listar(
        [FromQuery] int? categoriaId,
        [FromQuery] string? estadoStock,
        CancellationToken cancellationToken)
    {
        var productos = await _sender.Send(
            new ListarProductosQuery(categoriaId, estadoStock),
            cancellationToken);

        return Ok(productos.Select(producto => producto.ToResponse()).ToArray());
    }

    [HttpGet("stock-bajo")]
    [ProducesResponseType(typeof(ProductoResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoResponse>>> ListarStockBajo(
        CancellationToken cancellationToken)
    {
        var productos = await _sender.Send(
            new ListarProductosStockBajoQuery(),
            cancellationToken);

        return Ok(productos.Select(producto => producto.ToResponse()).ToArray());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var producto = await _sender.Send(
            new ObtenerProductoPorIdQuery(id),
            cancellationToken);

        return Ok(producto.ToResponse());
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductoResponse>> Crear(
        [FromBody] CrearProductoRequest request,
        CancellationToken cancellationToken)
    {
        var producto = await _sender.Send(
            request.ToCommand(),
            cancellationToken);
        var response = producto.ToResponse();

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoResponse>> Actualizar(
        int id,
        [FromBody] ActualizarProductoRequest request,
        CancellationToken cancellationToken)
    {
        var producto = await _sender.Send(
            request.ToCommand(id),
            cancellationToken);

        return Ok(producto.ToResponse());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new EliminarProductoCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/stock")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AjusteStockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AjusteStockResponse>> AjustarStock(
        int id,
        [FromBody] AjustarStockRequest request,
        CancellationToken cancellationToken)
    {
        var ajuste = await _sender.Send(request.ToCommand(id), cancellationToken);
        return Ok(ajuste.ToResponse());
    }
}

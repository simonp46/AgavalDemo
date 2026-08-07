using GestorInventario.Api.Contracts.Requests;
using GestorInventario.Api.Contracts.Responses;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Productos.Commands.ActualizarProducto;
using GestorInventario.Application.Productos.Commands.AjustarStock;
using GestorInventario.Application.Productos.Commands.CrearProducto;

namespace GestorInventario.Api.Contracts;

internal static class ApiContractMappings
{
    public static CrearProductoCommand ToCommand(this CrearProductoRequest request)
    {
        return new CrearProductoCommand(
            request.Nombre ?? string.Empty,
            request.Descripcion,
            request.Precio.GetValueOrDefault(),
            request.Stock.GetValueOrDefault(),
            request.StockMinimo.GetValueOrDefault(),
            request.CategoriaId.GetValueOrDefault());
    }

    public static ActualizarProductoCommand ToCommand(
        this ActualizarProductoRequest request,
        int id)
    {
        return new ActualizarProductoCommand(
            id,
            request.Nombre ?? string.Empty,
            request.Descripcion,
            request.Precio.GetValueOrDefault(),
            request.Stock.GetValueOrDefault(),
            request.StockMinimo.GetValueOrDefault(),
            request.CategoriaId.GetValueOrDefault());
    }

    public static AjustarStockCommand ToCommand(
        this AjustarStockRequest request,
        int id)
    {
        return new AjustarStockCommand(
            id,
            request.Tipo ?? string.Empty,
            request.Cantidad.GetValueOrDefault());
    }

    public static ProductoResponse ToResponse(this ProductoDto producto)
    {
        return new ProductoResponse(
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.StockMinimo,
            producto.CategoriaId,
            producto.CategoriaNombre,
            producto.FechaCreacion,
            producto.EsStockBajo);
    }

    public static CategoriaResponse ToResponse(this CategoriaDto categoria)
    {
        return new CategoriaResponse(
            categoria.Id,
            categoria.Nombre,
            categoria.Activo);
    }

    public static AjusteStockResponse ToResponse(this AjusteStockDto ajuste)
    {
        return new AjusteStockResponse(
            ajuste.ProductoId,
            ajuste.StockAnterior,
            ajuste.Tipo,
            ajuste.Cantidad,
            ajuste.StockActual,
            ajuste.EsStockBajo);
    }
}

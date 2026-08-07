using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Productos.Commands.CrearProducto;

public sealed record CrearProductoCommand(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int StockMinimo,
    int CategoriaId) : ICommand<ProductoDto>;

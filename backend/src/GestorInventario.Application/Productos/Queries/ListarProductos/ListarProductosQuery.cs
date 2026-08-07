using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Productos.Queries.ListarProductos;

public sealed record ListarProductosQuery(
    int? CategoriaId,
    string? EstadoStock) : IQuery<IReadOnlyList<ProductoDto>>;

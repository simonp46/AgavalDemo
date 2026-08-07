using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Productos.Queries.ListarProductosStockBajo;

public sealed record ListarProductosStockBajoQuery
    : IQuery<IReadOnlyList<ProductoDto>>;

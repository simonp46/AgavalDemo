using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Productos.Queries.ObtenerProductoPorId;

public sealed record ObtenerProductoPorIdQuery(int Id) : IQuery<ProductoDto>;

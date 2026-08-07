using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Categorias.Queries.ListarCategorias;

public sealed record ListarCategoriasQuery
    : IQuery<IReadOnlyList<CategoriaDto>>;

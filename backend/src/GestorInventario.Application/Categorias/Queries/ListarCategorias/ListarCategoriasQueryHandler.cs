using GestorInventario.Application.Dtos;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using MediatR;

namespace GestorInventario.Application.Categorias.Queries.ListarCategorias;

public sealed class ListarCategoriasQueryHandler
    : IRequestHandler<ListarCategoriasQuery, IReadOnlyList<CategoriaDto>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<CategoriaDto>> Handle(
        ListarCategoriasQuery request,
        CancellationToken cancellationToken)
    {
        var categorias = await _categoriaRepository.ListarAsync(cancellationToken);
        return categorias.Select(categoria => categoria.ToDto()).ToArray();
    }
}

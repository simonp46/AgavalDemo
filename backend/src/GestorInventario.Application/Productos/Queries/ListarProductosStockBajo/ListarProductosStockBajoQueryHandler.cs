using GestorInventario.Application.Dtos;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using MediatR;

namespace GestorInventario.Application.Productos.Queries.ListarProductosStockBajo;

public sealed class ListarProductosStockBajoQueryHandler
    : IRequestHandler<ListarProductosStockBajoQuery, IReadOnlyList<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarProductosStockBajoQueryHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<ProductoDto>> Handle(
        ListarProductosStockBajoQuery request,
        CancellationToken cancellationToken)
    {
        var productos = await _productoRepository.ListarAsync(
            categoriaId: null,
            stockBajo: true,
            cancellationToken);
        var categorias = await _categoriaRepository.ListarAsync(cancellationToken);
        var nombresCategorias = categorias.ToDictionary(
            categoria => categoria.Id,
            categoria => categoria.Nombre);

        return productos
            .Select(producto => producto.ToDto(
                nombresCategorias.TryGetValue(producto.CategoriaId, out var nombre)
                    ? nombre
                    : throw new InvalidOperationException(
                        $"La categoria {producto.CategoriaId} del producto " +
                        $"{producto.Id} no existe.")))
            .ToArray();
    }
}

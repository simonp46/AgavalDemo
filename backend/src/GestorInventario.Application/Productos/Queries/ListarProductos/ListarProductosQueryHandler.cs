using GestorInventario.Application.Dtos;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using MediatR;

namespace GestorInventario.Application.Productos.Queries.ListarProductos;

public sealed class ListarProductosQueryHandler
    : IRequestHandler<ListarProductosQuery, IReadOnlyList<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarProductosQueryHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<ProductoDto>> Handle(
        ListarProductosQuery request,
        CancellationToken cancellationToken)
    {
        var stockBajo = request.EstadoStock switch
        {
            "bajo" => true,
            "normal" => false,
            _ => (bool?)null
        };

        var productos = await _productoRepository.ListarAsync(
            request.CategoriaId,
            stockBajo,
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

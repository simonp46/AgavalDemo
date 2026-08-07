using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using MediatR;

namespace GestorInventario.Application.Productos.Queries.ObtenerProductoPorId;

public sealed class ObtenerProductoPorIdQueryHandler
    : IRequestHandler<ObtenerProductoPorIdQuery, ProductoDto>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ObtenerProductoPorIdQueryHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<ProductoDto> Handle(
        ObtenerProductoPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdSoloLecturaAsync(
            request.Id,
            cancellationToken);

        if (producto is null)
        {
            throw new NotFoundException(
                "product_not_found",
                $"No existe un producto con id {request.Id}.");
        }

        var categoria = await _categoriaRepository.ObtenerPorIdAsync(
            producto.CategoriaId,
            cancellationToken);

        if (categoria is null)
        {
            throw new InvalidOperationException(
                $"La categoria {producto.CategoriaId} del producto {producto.Id} no existe.");
        }

        return producto.ToDto(categoria.Nombre);
    }
}

using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using MediatR;

namespace GestorInventario.Application.Productos.Commands.ActualizarProducto;

public sealed class ActualizarProductoCommandHandler
    : IRequestHandler<ActualizarProductoCommand, ProductoDto>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarProductoCommandHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductoDto> Handle(
        ActualizarProductoCommand request,
        CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(
            request.Id,
            cancellationToken);

        if (producto is null)
        {
            throw new NotFoundException(
                "product_not_found",
                $"No existe un producto con id {request.Id}.");
        }

        var categoria = await _categoriaRepository.ObtenerPorIdAsync(
            request.CategoriaId,
            cancellationToken);

        if (categoria is null)
        {
            throw new CategoryReferenceInvalidException(request.CategoriaId);
        }

        producto.Actualizar(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.StockMinimo,
            request.CategoriaId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return producto.ToDto(categoria.Nombre);
    }
}

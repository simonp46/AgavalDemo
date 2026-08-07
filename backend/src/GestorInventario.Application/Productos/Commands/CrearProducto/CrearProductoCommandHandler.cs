using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Mappings;
using GestorInventario.Domain.Productos;
using MediatR;

namespace GestorInventario.Application.Productos.Commands.CrearProducto;

public sealed class CrearProductoCommandHandler
    : IRequestHandler<CrearProductoCommand, ProductoDto>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearProductoCommandHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductoDto> Handle(
        CrearProductoCommand request,
        CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(
            request.CategoriaId,
            cancellationToken);

        if (categoria is null)
        {
            throw new CategoryReferenceInvalidException(request.CategoriaId);
        }

        var producto = Producto.Crear(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.StockMinimo,
            request.CategoriaId);

        await _productoRepository.AgregarAsync(producto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return producto.ToDto(categoria.Nombre);
    }
}

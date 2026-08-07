using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using MediatR;

namespace GestorInventario.Application.Productos.Commands.EliminarProducto;

public sealed class EliminarProductoCommandHandler
    : IRequestHandler<EliminarProductoCommand>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarProductoCommandHandler(
        IProductoRepository productoRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        EliminarProductoCommand request,
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

        _productoRepository.Eliminar(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Productos;
using MediatR;

namespace GestorInventario.Application.Productos.Commands.AjustarStock;

public sealed class AjustarStockCommandHandler
    : IRequestHandler<AjustarStockCommand, AjusteStockDto>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AjustarStockCommandHandler(
        IProductoRepository productoRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<AjusteStockDto> Handle(
        AjustarStockCommand request,
        CancellationToken cancellationToken)
    {
        return _unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                var producto = await _productoRepository.ObtenerParaAjusteAsync(
                    request.Id,
                    token);

                if (producto is null)
                {
                    throw new NotFoundException(
                        "product_not_found",
                        $"No existe un producto con id {request.Id}.");
                }

                var tipo = request.Tipo switch
                {
                    "ENTRADA" => TipoMovimiento.Entrada,
                    "SALIDA" => TipoMovimiento.Salida,
                    _ => throw new ValidationException(
                        new Dictionary<string, string[]>
                        {
                            ["tipo"] = ["El tipo debe ser ENTRADA o SALIDA."]
                        })
                };

                if (tipo == TipoMovimiento.Salida && request.Cantidad > producto.Stock)
                {
                    throw new ConflictException(
                        "insufficient_stock",
                        "La salida solicitada supera el stock disponible.");
                }

                var stockAnterior = producto.Stock;
                producto.AjustarStock(tipo, request.Cantidad);
                await _unitOfWork.SaveChangesAsync(token);

                return new AjusteStockDto(
                    producto.Id,
                    stockAnterior,
                    request.Tipo,
                    request.Cantidad,
                    producto.Stock,
                    producto.EsStockBajo);
            },
            cancellationToken);
    }
}

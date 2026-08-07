using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Productos.Commands.AjustarStock;

public sealed record AjustarStockCommand(
    int Id,
    string Tipo,
    int Cantidad) : ICommand<AjusteStockDto>;

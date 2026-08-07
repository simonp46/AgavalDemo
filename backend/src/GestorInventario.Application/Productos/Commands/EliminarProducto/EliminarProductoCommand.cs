using GestorInventario.Application.Common.Messaging;

namespace GestorInventario.Application.Productos.Commands.EliminarProducto;

public sealed record EliminarProductoCommand(int Id) : ICommand;

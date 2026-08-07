using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Commands.CrearProducto;

public sealed class CrearProductoCommandValidator
    : IRequestValidator<CrearProductoCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(CrearProductoCommand request)
    {
        return ProductoInputValidation.Validate(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.CategoriaId);
    }
}

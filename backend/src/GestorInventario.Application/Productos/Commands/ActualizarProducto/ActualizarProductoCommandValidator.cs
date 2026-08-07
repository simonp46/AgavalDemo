using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Commands.ActualizarProducto;

public sealed class ActualizarProductoCommandValidator
    : IRequestValidator<ActualizarProductoCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(
        ActualizarProductoCommand request)
    {
        var errors = new ValidationErrors();

        if (request.Id <= 0)
        {
            errors.Add("id", "El identificador del producto debe ser positivo.");
        }

        foreach (var error in ProductoInputValidation.Validate(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.CategoriaId))
        {
            foreach (var message in error.Value)
            {
                errors.Add(error.Key, message);
            }
        }

        return errors.ToDictionary();
    }
}

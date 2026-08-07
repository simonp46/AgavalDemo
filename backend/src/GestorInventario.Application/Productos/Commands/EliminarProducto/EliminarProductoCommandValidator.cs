using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Commands.EliminarProducto;

public sealed class EliminarProductoCommandValidator
    : IRequestValidator<EliminarProductoCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(EliminarProductoCommand request)
    {
        var errors = new ValidationErrors();

        if (request.Id <= 0)
        {
            errors.Add("id", "El identificador del producto debe ser positivo.");
        }

        return errors.ToDictionary();
    }
}

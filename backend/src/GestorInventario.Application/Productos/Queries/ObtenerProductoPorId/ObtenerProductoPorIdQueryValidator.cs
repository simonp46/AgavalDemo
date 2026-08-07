using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Queries.ObtenerProductoPorId;

public sealed class ObtenerProductoPorIdQueryValidator
    : IRequestValidator<ObtenerProductoPorIdQuery>
{
    public IReadOnlyDictionary<string, string[]> Validate(
        ObtenerProductoPorIdQuery request)
    {
        var errors = new ValidationErrors();

        if (request.Id <= 0)
        {
            errors.Add("id", "El identificador del producto debe ser positivo.");
        }

        return errors.ToDictionary();
    }
}

using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Queries.ListarProductos;

public sealed class ListarProductosQueryValidator
    : IRequestValidator<ListarProductosQuery>
{
    private static readonly string[] EstadosPermitidos = ["bajo", "normal"];

    public IReadOnlyDictionary<string, string[]> Validate(ListarProductosQuery request)
    {
        var errors = new ValidationErrors();

        if (request.CategoriaId is <= 0)
        {
            errors.Add("categoriaId", "La categoria debe ser un identificador positivo.");
        }

        if (request.EstadoStock is not null &&
            !EstadosPermitidos.Contains(request.EstadoStock, StringComparer.Ordinal))
        {
            errors.Add("estadoStock", "El estado de stock debe ser bajo o normal.");
        }

        return errors.ToDictionary();
    }
}

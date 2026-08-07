using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Productos.Commands.AjustarStock;

public sealed class AjustarStockCommandValidator
    : IRequestValidator<AjustarStockCommand>
{
    private static readonly string[] TiposPermitidos = ["ENTRADA", "SALIDA"];

    public IReadOnlyDictionary<string, string[]> Validate(AjustarStockCommand request)
    {
        var errors = new ValidationErrors();

        if (request.Id <= 0)
        {
            errors.Add("id", "El identificador del producto debe ser positivo.");
        }

        if (!TiposPermitidos.Contains(request.Tipo, StringComparer.Ordinal))
        {
            errors.Add("tipo", "El tipo debe ser ENTRADA o SALIDA.");
        }

        if (request.Cantidad <= 0)
        {
            errors.Add("cantidad", "La cantidad debe ser mayor que cero.");
        }

        return errors.ToDictionary();
    }
}

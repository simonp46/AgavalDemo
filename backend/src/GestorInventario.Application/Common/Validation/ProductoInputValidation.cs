using GestorInventario.Domain.Productos;

namespace GestorInventario.Application.Common.Validation;

public static class ProductoInputValidation
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        string? nombre,
        string? descripcion,
        decimal precio,
        int stock,
        int categoriaId)
    {
        var errors = new ValidationErrors();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errors.Add("nombre", "El nombre es obligatorio.");
        }
        else if (nombre.Trim().Length > Producto.NombreMaximo)
        {
            errors.Add(
                "nombre",
                $"El nombre no puede superar {Producto.NombreMaximo} caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(descripcion) &&
            descripcion.Trim().Length > Producto.DescripcionMaxima)
        {
            errors.Add(
                "descripcion",
                $"La descripcion no puede superar {Producto.DescripcionMaxima} caracteres.");
        }

        if (precio <= 0)
        {
            errors.Add("precio", "El precio debe ser mayor que cero.");
        }
        else if (precio > Producto.PrecioMaximo)
        {
            errors.Add(
                "precio",
                $"El precio no puede superar {Producto.PrecioMaximo}.");
        }
        else if (decimal.Round(precio, 2) != precio)
        {
            errors.Add("precio", "El precio no puede tener mas de dos decimales.");
        }

        if (stock < 0)
        {
            errors.Add("stock", "El stock no puede ser negativo.");
        }

        if (categoriaId <= 0)
        {
            errors.Add("categoriaId", "La categoria debe ser un identificador positivo.");
        }

        return errors.ToDictionary();
    }
}

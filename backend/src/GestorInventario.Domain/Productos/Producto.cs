using GestorInventario.Domain.Common;

namespace GestorInventario.Domain.Productos;

public sealed class Producto
{
    public const int NombreMaximo = 150;
    public const int DescripcionMaxima = 500;
    public const decimal PrecioMaximo = 99_999_999.99m;

    private Producto()
    {
    }

    private Producto(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        int stockMinimo,
        int categoriaId)
    {
        EstablecerDatos(nombre, descripcion, precio, stock, stockMinimo, categoriaId);
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string? Descripcion { get; private set; }

    public decimal Precio { get; private set; }

    public int Stock { get; private set; }

    public int StockMinimo { get; private set; }

    public int CategoriaId { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public bool EsStockBajo => Stock < StockMinimo;

    public static Producto Crear(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        int stockMinimo,
        int categoriaId)
    {
        return new Producto(
            nombre,
            descripcion,
            precio,
            stock,
            stockMinimo,
            categoriaId);
    }

    public void Actualizar(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        int stockMinimo,
        int categoriaId)
    {
        EstablecerDatos(nombre, descripcion, precio, stock, stockMinimo, categoriaId);
    }

    public void AjustarStock(TipoMovimiento tipo, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new DomainException("La cantidad del ajuste debe ser mayor que cero.");
        }

        try
        {
            var nuevoStock = tipo switch
            {
                TipoMovimiento.Entrada => checked(Stock + cantidad),
                TipoMovimiento.Salida => Stock - cantidad,
                _ => throw new DomainException("El tipo de movimiento no es valido.")
            };

            ValidarStock(nuevoStock);
            Stock = nuevoStock;
        }
        catch (OverflowException exception)
        {
            throw new DomainException("El ajuste supera el rango permitido para el stock.", exception);
        }
    }

    private void EstablecerDatos(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        int stockMinimo,
        int categoriaId)
    {
        Nombre = ValidarNombre(nombre);
        Descripcion = ValidarDescripcion(descripcion);
        Precio = ValidarPrecio(precio);
        ValidarStock(stock);
        ValidarCategoria(categoriaId);

        Stock = stock;
        StockMinimo = stockMinimo;
        CategoriaId = categoriaId;
    }

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre del producto es obligatorio.");
        }

        var nombreNormalizado = nombre.Trim();

        if (nombreNormalizado.Length > NombreMaximo)
        {
            throw new DomainException(
                $"El nombre del producto no puede superar {NombreMaximo} caracteres.");
        }

        return nombreNormalizado;
    }

    private static string? ValidarDescripcion(string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return null;
        }

        var descripcionNormalizada = descripcion.Trim();

        if (descripcionNormalizada.Length > DescripcionMaxima)
        {
            throw new DomainException(
                $"La descripcion no puede superar {DescripcionMaxima} caracteres.");
        }

        return descripcionNormalizada;
    }

    private static decimal ValidarPrecio(decimal precio)
    {
        if (precio <= 0)
        {
            throw new DomainException("El precio debe ser mayor que cero.");
        }

        if (precio > PrecioMaximo)
        {
            throw new DomainException($"El precio no puede superar {PrecioMaximo}.");
        }

        if (decimal.Round(precio, 2) != precio)
        {
            throw new DomainException("El precio no puede tener mas de dos decimales.");
        }

        return precio;
    }

    private static void ValidarStock(int stock)
    {
        if (stock < 0)
        {
            throw new DomainException("El stock no puede ser negativo.");
        }
    }

    private static void ValidarCategoria(int categoriaId)
    {
        if (categoriaId <= 0)
        {
            throw new DomainException("La categoria del producto no es valida.");
        }
    }
}

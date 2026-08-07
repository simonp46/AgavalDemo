using GestorInventario.Domain.Common;
using GestorInventario.Domain.Productos;

namespace GestorInventario.Domain.Tests;

public sealed class ProductoTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaDatosYCalculaStockBajo()
    {
        var producto = Producto.Crear(
            "  Teclado  ",
            "  Mecanico  ",
            100.50m,
            2,
            5,
            1);

        Assert.Equal("Teclado", producto.Nombre);
        Assert.Equal("Mecanico", producto.Descripcion);
        Assert.True(producto.EsStockBajo);
    }

    [Fact]
    public void Crear_ConStockMinimoCero_ConservaValorExplicito()
    {
        var producto = Producto.Crear("Producto", null, 1m, 0, 0, 1);

        Assert.Equal(0, producto.StockMinimo);
        Assert.False(producto.EsStockBajo);
    }

    [Fact]
    public void Crear_SinNombre_RechazaProducto()
    {
        var exception = Assert.Throws<DomainException>(
            () => Producto.Crear(" ", null, 1m, 0, 5, 1));

        Assert.Contains("obligatorio", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Crear_ConPrecioNoPositivo_RechazaProducto()
    {
        var exception = Assert.Throws<DomainException>(
            () => Producto.Crear("Producto", null, 0m, 0, 5, 1));

        Assert.Contains("mayor que cero", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Crear_ConStockNegativo_RechazaProducto()
    {
        var exception = Assert.Throws<DomainException>(
            () => Producto.Crear("Producto", null, 1m, -1, 5, 1));

        Assert.Contains("negativo", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AjustarStock_EntradaYSALIDA_ActualizaStock()
    {
        var producto = Producto.Crear("Producto", null, 1m, 5, 2, 1);

        producto.AjustarStock(TipoMovimiento.Entrada, 3);
        producto.AjustarStock(TipoMovimiento.Salida, 2);

        Assert.Equal(6, producto.Stock);
    }

    [Fact]
    public void AjustarStock_ConCantidadNoPositiva_RechazaAjuste()
    {
        var producto = Producto.Crear("Producto", null, 1m, 5, 2, 1);

        var exception = Assert.Throws<DomainException>(
            () => producto.AjustarStock(TipoMovimiento.Entrada, 0));

        Assert.Contains("mayor que cero", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AjustarStock_SalidaSuperiorAlStock_RechazaAjuste()
    {
        var producto = Producto.Crear("Producto", null, 1m, 1, 2, 1);

        Assert.Throws<DomainException>(
            () => producto.AjustarStock(TipoMovimiento.Salida, 2));
        Assert.Equal(1, producto.Stock);
    }
}

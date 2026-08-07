using GestorInventario.Application.Dtos;
using GestorInventario.Domain.Categorias;
using GestorInventario.Domain.Productos;

namespace GestorInventario.Application.Mappings;

public static class DomainMappings
{
    public static ProductoDto ToDto(this Producto producto, string categoriaNombre)
    {
        return new ProductoDto(
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.StockMinimo,
            producto.CategoriaId,
            categoriaNombre,
            producto.FechaCreacion,
            producto.EsStockBajo);
    }

    public static CategoriaDto ToDto(this Categoria categoria)
    {
        return new CategoriaDto(
            categoria.Id,
            categoria.Nombre,
            categoria.Activo);
    }
}

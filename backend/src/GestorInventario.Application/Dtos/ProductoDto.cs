namespace GestorInventario.Application.Dtos;

public sealed record ProductoDto(
    int Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int StockMinimo,
    int CategoriaId,
    string CategoriaNombre,
    DateTime FechaCreacion,
    bool EsStockBajo);

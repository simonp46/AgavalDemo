namespace GestorInventario.Api.Contracts.Responses;

public sealed record ProductoResponse(
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

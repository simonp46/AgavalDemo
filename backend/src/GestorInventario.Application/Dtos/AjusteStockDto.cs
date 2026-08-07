namespace GestorInventario.Application.Dtos;

public sealed record AjusteStockDto(
    int ProductoId,
    int StockAnterior,
    string Tipo,
    int Cantidad,
    int StockActual,
    bool EsStockBajo);

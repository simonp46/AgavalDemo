namespace GestorInventario.Api.Contracts.Responses;

public sealed record AjusteStockResponse(
    int ProductoId,
    int StockAnterior,
    string Tipo,
    int Cantidad,
    int StockActual,
    bool EsStockBajo);

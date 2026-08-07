using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Api.Contracts.Requests;

public sealed class AjustarStockRequest
{
    [Required(AllowEmptyStrings = false)]
    public string? Tipo { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? Cantidad { get; init; }
}

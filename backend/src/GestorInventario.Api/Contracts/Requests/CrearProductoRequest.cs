using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Api.Contracts.Requests;

public sealed class CrearProductoRequest
{
    private const int NombreMaximo = 150;
    private const int DescripcionMaxima = 500;

    [Required(AllowEmptyStrings = false)]
    [StringLength(NombreMaximo)]
    public string? Nombre { get; init; }

    [StringLength(DescripcionMaxima)]
    public string? Descripcion { get; init; }

    [Required]
    [Range(
        typeof(decimal),
        "0.01",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "El precio debe ser mayor que cero y compatible con DECIMAL(10,2).")]
    public decimal? Precio { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int? Stock { get; init; }

    [Required]
    public int? StockMinimo { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? CategoriaId { get; init; }
}

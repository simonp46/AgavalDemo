using System.ComponentModel.DataAnnotations;
using DomainUsuario = GestorInventario.Domain.Usuarios.Usuario;

namespace GestorInventario.Api.Contracts.Requests;

public sealed class RegistrarUsuarioRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.NombreMaximo)]
    public string? Nombre { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.ApellidosMaximo)]
    public string? Apellidos { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.NumeroDocumentoMaximo)]
    public string? NumeroDocumento { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.AreaMaxima)]
    public string? Area { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.NombreUsuarioMaximo, MinimumLength = 4)]
    public string? Usuario { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(128, MinimumLength = 8)]
    public string? Contrasena { get; init; }
}

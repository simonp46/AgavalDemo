using System.ComponentModel.DataAnnotations;
using DomainUsuario = GestorInventario.Domain.Usuarios.Usuario;

namespace GestorInventario.Api.Contracts.Requests;

public sealed class LoginRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(DomainUsuario.NombreUsuarioMaximo)]
    public string? Usuario { get; init; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(128)]
    public string? Contrasena { get; init; }
}

using GestorInventario.Api.Contracts.Requests;
using GestorInventario.Api.Contracts.Responses;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Usuarios.Commands.IniciarSesion;
using GestorInventario.Application.Usuarios.Commands.RegistrarUsuario;

namespace GestorInventario.Api.Contracts;

internal static class AuthContractMappings
{
    public static IniciarSesionCommand ToCommand(this LoginRequest request)
    {
        return new IniciarSesionCommand(
            request.Usuario!,
            request.Contrasena!);
    }

    public static RegistrarUsuarioCommand ToCommand(
        this RegistrarUsuarioRequest request)
    {
        return new RegistrarUsuarioCommand(
            request.Nombre!,
            request.Apellidos!,
            request.NumeroDocumento!,
            request.Area!,
            request.Usuario!,
            request.Contrasena!);
    }

    public static UsuarioResponse ToResponse(this UsuarioDto usuario)
    {
        return new UsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellidos,
            usuario.NumeroDocumento,
            usuario.Area,
            usuario.NombreUsuario,
            usuario.FechaCreacion);
    }
}

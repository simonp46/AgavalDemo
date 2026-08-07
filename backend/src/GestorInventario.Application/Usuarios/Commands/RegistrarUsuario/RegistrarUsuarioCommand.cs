using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Usuarios.Commands.RegistrarUsuario;

public sealed record RegistrarUsuarioCommand(
    string Nombre,
    string Apellidos,
    string NumeroDocumento,
    string Area,
    string Usuario,
    string Contrasena) : ICommand<UsuarioDto>;

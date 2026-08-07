using GestorInventario.Application.Common.Messaging;
using GestorInventario.Application.Dtos;

namespace GestorInventario.Application.Usuarios.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(
    string Usuario,
    string Contrasena) : ICommand<UsuarioDto>;

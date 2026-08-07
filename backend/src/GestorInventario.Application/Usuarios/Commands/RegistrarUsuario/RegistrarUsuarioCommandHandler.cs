using GestorInventario.Application.Dtos;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using MediatR;

namespace GestorInventario.Application.Usuarios.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioCommandHandler
    : IRequestHandler<RegistrarUsuarioCommand, UsuarioDto>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegistrarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public Task<UsuarioDto> Handle(
        RegistrarUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var passwordHash = _passwordHasher.Hash(request.Contrasena);
        var usuario = GestorInventario.Domain.Usuarios.Usuario.Crear(
            request.Nombre,
            request.Apellidos,
            request.NumeroDocumento,
            request.Area,
            request.Usuario,
            passwordHash);

        return _usuarioRepository.RegistrarAsync(
            usuario,
            ModuloAcceso.Productos,
            cancellationToken);
    }
}

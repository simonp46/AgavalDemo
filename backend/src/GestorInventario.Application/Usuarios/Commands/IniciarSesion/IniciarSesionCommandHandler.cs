using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using MediatR;

namespace GestorInventario.Application.Usuarios.Commands.IniciarSesion;

public sealed class IniciarSesionCommandHandler
    : IRequestHandler<IniciarSesionCommand, UsuarioDto>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public IniciarSesionCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioDto> Handle(
        IniciarSesionCommand request,
        CancellationToken cancellationToken)
    {
        var nombreUsuario = Usuario.NormalizarNombreUsuario(request.Usuario);
        var credenciales = await _usuarioRepository.ObtenerCredencialesAsync(
            nombreUsuario,
            ModuloAcceso.Productos,
            cancellationToken);

        if (credenciales is null ||
            !credenciales.Activo ||
            !credenciales.TieneAcceso ||
            !_passwordHasher.Verify(credenciales.PasswordHash, request.Contrasena))
        {
            throw new AuthenticationFailedException();
        }

        return credenciales.Usuario;
    }
}

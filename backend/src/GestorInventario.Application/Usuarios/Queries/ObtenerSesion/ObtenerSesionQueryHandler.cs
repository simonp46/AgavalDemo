using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using MediatR;

namespace GestorInventario.Application.Usuarios.Queries.ObtenerSesion;

public sealed class ObtenerSesionQueryHandler
    : IRequestHandler<ObtenerSesionQuery, UsuarioDto>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObtenerSesionQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioDto> Handle(
        ObtenerSesionQuery request,
        CancellationToken cancellationToken)
    {
        var acceso = await _usuarioRepository.ObtenerPorIdAsync(
            request.UsuarioId,
            ModuloAcceso.Productos,
            cancellationToken);

        if (acceso is null || !acceso.Activo || !acceso.TieneAcceso)
        {
            throw new AuthenticationRequiredException();
        }

        return acceso.Usuario;
    }
}

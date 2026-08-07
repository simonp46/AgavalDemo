using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using MediatR;

namespace GestorInventario.Application.Usuarios.Queries.SugerirNombreUsuario;

public sealed class SugerirNombreUsuarioQueryHandler
    : IRequestHandler<SugerirNombreUsuarioQuery, string>
{
    private readonly IUsuarioRepository _usuarioRepository;

    public SugerirNombreUsuarioQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public Task<string> Handle(
        SugerirNombreUsuarioQuery request,
        CancellationToken cancellationToken)
    {
        var baseUsuario = Usuario.CrearBaseSugerencia(
            request.Nombre,
            request.Apellidos);

        return _usuarioRepository.SugerirNombreUsuarioAsync(
            baseUsuario,
            cancellationToken);
    }
}

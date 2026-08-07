using GestorInventario.Application.Dtos;
using GestorInventario.Domain.Usuarios;

namespace GestorInventario.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<UsuarioDto> RegistrarAsync(
        Usuario usuario,
        string moduloCodigo,
        CancellationToken cancellationToken = default);

    Task<UsuarioCredencialesDto?> ObtenerCredencialesAsync(
        string nombreUsuario,
        string moduloCodigo,
        CancellationToken cancellationToken = default);

    Task<UsuarioAccesoDto?> ObtenerPorIdAsync(
        int usuarioId,
        string moduloCodigo,
        CancellationToken cancellationToken = default);

    Task<string> SugerirNombreUsuarioAsync(
        string baseUsuario,
        CancellationToken cancellationToken = default);

    Task<bool> TieneAccesoModuloAsync(
        int usuarioId,
        string moduloCodigo,
        CancellationToken cancellationToken = default);
}

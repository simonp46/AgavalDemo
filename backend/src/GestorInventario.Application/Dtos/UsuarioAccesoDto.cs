namespace GestorInventario.Application.Dtos;

public sealed record UsuarioAccesoDto(
    UsuarioDto Usuario,
    bool Activo,
    bool TieneAcceso);

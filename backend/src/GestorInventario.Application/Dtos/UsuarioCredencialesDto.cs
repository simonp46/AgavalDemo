namespace GestorInventario.Application.Dtos;

public sealed record UsuarioCredencialesDto(
    UsuarioDto Usuario,
    string PasswordHash,
    bool Activo,
    bool TieneAcceso);

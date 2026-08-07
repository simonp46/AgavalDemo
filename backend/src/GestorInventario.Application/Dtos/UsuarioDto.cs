namespace GestorInventario.Application.Dtos;

public sealed record UsuarioDto(
    int Id,
    string Nombre,
    string Apellidos,
    string NumeroDocumento,
    string Area,
    string NombreUsuario,
    DateTime FechaCreacion);

namespace GestorInventario.Api.Contracts.Responses;

public sealed record UsuarioResponse(
    int Id,
    string Nombre,
    string Apellidos,
    string NumeroDocumento,
    string Area,
    string Usuario,
    DateTime FechaCreacion);

namespace GestorInventario.Api.Contracts.Responses;

public sealed record CategoriaResponse(
    int Id,
    string Nombre,
    bool Activo);
